using System.Collections.Generic;
using UnityEngine;
using HynesSight;

namespace HynesSight
{
	/// <summary>
	/// A simple EventBus implementation; only uses a string channel parameter to differentiate between event calls, and uses params-style arguments for simplicity.
	/// </summary>
	public static class EventBus
	{
		/* INSTRUCTIONS
		 * The EventBus stores delegates from other classes to be called at a specified time later.
		 * We do this by using 'channels', which are simple string keys to group delegates together under a unified event.
		 * 
		 * When Subscribe is called, you pass the channel name you would like to use along with the delegate itself.
		 * You can unsubscribe delegates with the Unsubscribe function.
		 * When Dispatch is called, any subscribed delegates in that channel are invoked.
		 * 
		 * This is a static class, not a Unity object, so it will hold information between scenes.
		 * Make sure you manually clean up events between scenes with Unsubscribe and CleanUpEvents.
		 * */

		private static Dictionary<string, List<DynamicParamsDelegate>> _subscribedEvents = new Dictionary<string, List<DynamicParamsDelegate>>();
		
		public static void Subscribe(string channelName, DynamicParamsDelegate inEvent)
		{
			if (!_subscribedEvents.ContainsKey(channelName))
			{
				_subscribedEvents.Add(channelName, new List<DynamicParamsDelegate>(1));
			}

			_subscribedEvents[channelName].Add(inEvent);
		}

		public static void Unsubscribe(string channelName, DynamicParamsDelegate inEvent)
		{
			if (!_subscribedEvents.ContainsKey(channelName) || !_subscribedEvents[channelName].Contains(inEvent))
			{
				Debug.LogError("Trying to unsubscribe an event that is not subscribed. Channel: " + channelName + ". Event: " + inEvent.Method + ".");
				return;
			}

			_subscribedEvents[channelName].Remove(inEvent);
		}

		public static void Dispatch(string channelName)
		{
			if (!_subscribedEvents.ContainsKey(channelName))
			{
				_subscribedEvents.Add(channelName, new List<DynamicParamsDelegate>(0));
				return;
			}

			if (_subscribedEvents[channelName].Count > 0)
			{
				for (int n = _subscribedEvents[channelName].Count - 1; n > 1; n--)
				{
					_subscribedEvents[channelName][n].Invoke();
				}
			}
		}

		public static void CleanEvents()
		{
			_subscribedEvents = new Dictionary<string, List<DynamicParamsDelegate>>();
		}
	}
}
