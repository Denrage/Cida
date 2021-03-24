using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Module.IrcAnime.Avalonia.AttachedProperties
{
    public class EventToCommand : AvaloniaObject
    {
        public static readonly AttachedProperty<ICommand> CommandProperty = AvaloniaProperty.RegisterAttached<EventToCommand, Interactive, ICommand>(
            "Command", default, false, BindingMode.OneTime, null, OnCommandChange);

        public static readonly AttachedProperty<string> EventNameProperty = AvaloniaProperty.RegisterAttached<EventToCommand, Interactive, string>(
            "EventName", default, false, BindingMode.OneWay, null, OnEventNameChange);

        private static ICommand OnCommandChange(IAvaloniaObject element, ICommand commandValue)
        {
            if (element != null && element is Interactive interactive)
            {
                UpdateConnection(interactive, GetEventName(interactive), commandValue);
            }

            return commandValue;
        }

        private static string OnEventNameChange(IAvaloniaObject element, string routedEvent)
        {
            if (element != null && element is Interactive interactive)
            {
                UpdateConnection(interactive, routedEvent, GetCommand(interactive));
            }

            return routedEvent;
        }

        private static void UpdateConnection(Interactive element, string eventName, ICommand command)
        {
            if (element is null || eventName is null)
            {
                return;
            }

            System.EventHandler handler = Handler;
            var eventInfo = element.GetType().GetEvent(eventName);

            if (command != null)
            {
                eventInfo.AddEventHandler(element, handler);
            }
            else
            {
                eventInfo.RemoveEventHandler(element, handler);
            }
        }

        private static void Handler(object s, EventArgs e)
        {
            if (s is AvaloniaObject avaloniaObject)
            {
                if (GetCommand(avaloniaObject)?.CanExecute(null) == true)
                {
                    GetCommand(avaloniaObject).Execute(null);
                }
            }
        }

        public static void SetCommand(AvaloniaObject element, ICommand commandValue)
        {
            element.SetValue(CommandProperty, commandValue);
        }

        public static ICommand GetCommand(AvaloniaObject element)
        {
            return element.GetValue(CommandProperty);
        }

        public static void SetEventName(AvaloniaObject element, string routedEvent)
        {
            element.SetValue(EventNameProperty, routedEvent);
        }

        public static string GetEventName(AvaloniaObject element)
        {
            return element.GetValue(EventNameProperty);
        }
    }
}
