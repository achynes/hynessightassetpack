using System.Collections.Generic;
using UnityEngine;

public static class ScrollRectExtensions
{
	[System.Serializable]
	public struct ScrollRectBuffer
	{
		public ScrollRectBuffer(float _top, float _bottom, float _left, float _right)
		{
			top = _top;
			bottom = _bottom;
			left = _left;
			right = _right;
		}

		public static ScrollRectBuffer operator *(ScrollRectBuffer _scrollRectBuffer, float _multiplier)
		{
			return new ScrollRectBuffer(_scrollRectBuffer.top * _multiplier, _scrollRectBuffer.bottom * _multiplier, _scrollRectBuffer.left * _multiplier, _scrollRectBuffer.right * _multiplier);
		}

		public float top;
		public float bottom;
		public float left;
		public float right;
	}

	// Jumps the view rect of the given ScrollRect component to center it over the specified element.
	// 
	// A Manual Buffer can be added to the element to jump past the element by the size specified, which may look better and provide the user with a view of what is next in the scroll view.
	// If specifying a Manual Buffer, use a normalized value (0 to 1) as a percentage of the Scroll Rect's view size.
	// If a null Manual Buffer is passed, the function will use twice the size of the selected element as a buffer, which is a nice default when all of the elements are the same size.
	public static void JumpScrollRectToElement(this UnityEngine.UI.ScrollRect _scrollRect, RectTransform _element, ScrollRectBuffer? _manualBuffer = null)
	{
		if (_element == null)
		{
			Debug.LogWarningFormat("Cannot jump ScrollRect {0} as a null _contentElement was given.", _scrollRect.GetFullScenePath());
			return;
		}

		RectTransform _contentRectTransform = _scrollRect.content;
		if (_element.IsChildOf(_contentRectTransform) == false)
		{
			Debug.LogWarningFormat("Cannot jump ScrollRect {0} to {1} as it is not an element of the ScrollRect content.", _scrollRect.GetFullScenePath(), _element.GetFullScenePath());
			return;
		}

		_scrollRect.StopMovement();

		// The function works with the ScrollRect component which has a Rect for the 'content' and another for the (partial/scrollable) 'view'.
		// Calculations have been simplified by normalizing everything into the content space, and in the end the result needs to be transformed onto the actual 'scrollable' bounds of the ScrollRect component.
		//
		// This diagram presents the spaces used, simplified as vertical only.
		//
		//	----------------- 1
		//	|				|
		//	|				|
		//	|	Content		|
		//	|	height 1	|
		//	|				|
		//	|				|															--- 1
		//	|				|															 |
		//	|				|															 |
		//	|				|															 |
		//	|				|															 |
		//	|				|															 |
		//	|				|															 |
		//	|				| 0.5														 |	<-- Scrollable bound has its own normalized space
		//	|				|															 |
		//	|				|				-----------------							 |
		//	|				|				|				|		View currently		 |
		//	|				|				|	View		|		positioned at 0		 |
		//	|				|				|	height .44	|		on Scrollable		 |
		//	|				|				|	relative	|		bounds				 |
		//	|				|				|	to content	|			<-------		---	0			
		//	|				|				|				|
		//	|				|				|				|
		//	|				|				|				|
		//	|				|				|				|
		//	|				|				|				|
		//	----------------- 0				-----------------
		//
		//________________________________________________________________________________________________________________________________________________________

		Rect _contentRect = _scrollRect.content.rect;
		Rect _viewRect = _scrollRect.viewport.rect;

		Rect _elementRectRelativeToContent = _element.rect.InterTransformRect(_element, _scrollRect.content);

		Vector2 _contentRectSize = _contentRect.size;

		// Calculate a 0 to 1 representation of the element's position relative to the content rect.
		Vector2 _elementPositionInContent = _elementRectRelativeToContent.position;
		_elementPositionInContent /= _contentRectSize;
		_elementPositionInContent += _contentRectTransform.pivot;

		Vector2 _elementSizeInContent = _elementRectRelativeToContent.size / _contentRectSize;
		ScrollRectBuffer _elementBuffer = _manualBuffer.GetValueOrDefault(new ScrollRectBuffer(_elementSizeInContent.x, _elementSizeInContent.y, _elementSizeInContent.x, _elementSizeInContent.y) * 2f);

		// Corners with added buffer in content space clockwise from top-left.
		Vector2[] _elementNormalizedCorners = new Vector2[]
		{
			_elementPositionInContent                                                                   + new Vector2(-_elementBuffer.left, _elementBuffer.top),
			_elementPositionInContent + new Vector2(_elementSizeInContent.x, 0f)                        + new Vector2(_elementBuffer.right, _elementBuffer.top),
			_elementPositionInContent + new Vector2(_elementSizeInContent.x, _elementSizeInContent.y)   + new Vector2(_elementBuffer.right, -_elementBuffer.bottom),
			_elementPositionInContent + new Vector2(0f, _elementSizeInContent.y)                        + new Vector2(-_elementBuffer.left, -_elementBuffer.bottom)
		};

		Vector2 _normalMiddle = new Vector2(.5f, .5f);

		// Pick the corner of the selected element furthest from the middle of the space to make sure the whole element is visible when we're done.
		Vector2 _chosenJumpCorner = _elementNormalizedCorners[0];
		Vector2 _furthestDistance = (_chosenJumpCorner - _normalMiddle).Abs();
		for (int n = 1; n < _elementNormalizedCorners.Length; ++n)
		{
			Vector2 _distanceToContentMiddle = (_elementNormalizedCorners[n] - _normalMiddle).Abs();

			if (_distanceToContentMiddle.x > _furthestDistance.x)
			{
				_chosenJumpCorner.x = _elementNormalizedCorners[n].x;
				_furthestDistance.x = _distanceToContentMiddle.x;
			}

			if (_distanceToContentMiddle.y > _furthestDistance.y)
			{
				_chosenJumpCorner.y = _elementNormalizedCorners[n].y;
				_furthestDistance.y = _distanceToContentMiddle.y;
			}
		}

		Rect _viewRectRelativeToContent = _viewRect.InterTransformRect(_scrollRect.viewport, _scrollRect.content);
		Vector2 _viewSizeInContent = _viewRectRelativeToContent.size / _contentRectSize;

		Vector2 _viewPositionInContent = _viewRectRelativeToContent.position;
		_viewPositionInContent /= _contentRectSize;
		_viewPositionInContent += _contentRectTransform.pivot;

		Vector2 _viewMaxInContent = _viewPositionInContent + _viewSizeInContent;

		// 0 means visible already, -1 means below the view and 1 means above.
		int _elementRelationToViewX = 0;
		int _elementRelationToViewY = 0;

		// @todo: In testing, there is some amount of discrepancy from the expected result. In some use cases, the jump doesn't kick in for elements that are just outside the view space.

		if (_chosenJumpCorner.x < _viewPositionInContent.x)
			_elementRelationToViewX = -1;
		else if (_chosenJumpCorner.x > _viewMaxInContent.x)
			_elementRelationToViewX = 1;

		if (_chosenJumpCorner.y < _viewPositionInContent.y)
			_elementRelationToViewY = -1;
		else if (_chosenJumpCorner.y > _viewMaxInContent.y)
			_elementRelationToViewY = 1;

		// This is where it gets finicky; the Scroll Rect scroll values go from 0 to 1 as well, but compared to the normalized space of the Content rect,
		// the 'scrollable space' is a smaller by the size of the view, because it is measured from the center of that view.
		// Make any 0s into 1s for division later, it will be benign anyway.
		Vector2 _scrollableSizeInContent = Vector2.one - _viewSizeInContent;
		if (_scrollableSizeInContent.x == 0)
			_scrollableSizeInContent.x = 1;
		if (_scrollableSizeInContent.y == 0)
			_scrollableSizeInContent.y = 1;

		// For the final position of the scroll, we need to offset by half the view size, so we move the view as little as possible to make the element visible.
		Vector2 _newPositionOffset = .5f * Vector2.Scale(_viewSizeInContent, new Vector2(_elementRelationToViewX, _elementRelationToViewY));

		// To move the selected element into scrollable space, we scale the difference between the middle of the space and the element (because the two spaces are aligned at the middle).
		Vector2 _scrollEdgePosition = _normalMiddle + ((_chosenJumpCorner - _newPositionOffset - _normalMiddle) / _scrollableSizeInContent);

		if (_elementRelationToViewX != 0)
			_scrollRect.horizontalNormalizedPosition = _scrollEdgePosition.x;

		if (_elementRelationToViewY != 0)
			_scrollRect.verticalNormalizedPosition = _scrollEdgePosition.y;
	}

}
