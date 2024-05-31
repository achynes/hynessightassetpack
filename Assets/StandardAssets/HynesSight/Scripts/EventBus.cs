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
		
		public static void Subscribe(string channelName_, DynamicParamsDelegate event_)
		{
			if (!_subscribedEvents.ContainsKey(channelName_))
			{
				_subscribedEvents.Add(channelName_, new List<DynamicParamsDelegate>(1));
			}

			_subscribedEvents[channelName_].Add(event_);
		}

		public static void Unsubscribe(string channelName_, DynamicParamsDelegate event_)
		{
			if (!_subscribedEvents.ContainsKey(channelName_) || !_subscribedEvents[channelName_].Contains(event_))
			{
				Debug.LogError("Trying to unsubscribe an event that is not subscribed. Channel: " + channelName_ + ". Event: " + event_.Method + ".");
				return;
			}

			_subscribedEvents[channelName_].Remove(event_);
		}

		public static void Dispatch(string channelName_)
		{
			if (!_subscribedEvents.ContainsKey(channelName_))
			{
				_subscribedEvents.Add(channelName_, new List<DynamicParamsDelegate>(0));
				return;
			}

			if (_subscribedEvents[channelName_].Count > 0)
			{
				for (int n = _subscribedEvents[channelName_].Count - 1; n > 1; n--)
				{
					_subscribedEvents[channelName_][n].Invoke();
				}
			}
		}

		public static void CleanEvents()
		{
			_subscribedEvents = new Dictionary<string, List<DynamicParamsDelegate>>();
		}
	}
}
