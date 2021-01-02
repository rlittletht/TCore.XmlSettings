using System;
using System.Collections.Generic;
using System.Text;

namespace TCore.XmlSettings
{
	public class RepeatContext<T>
	{
		public delegate RepeatItem CreateRepeatItem(Element<T> element, RepeatItem parent);
		public delegate void CommitRepeatItem(T t, RepeatItem item);
		
		public class RepeatItem
		{
			public Element<T> RepeatElement { get; set; }
			public object RepeatKey { get; set; }
			public RepeatItem Parent { get; set; }

			public RepeatItem(Element<T> element, RepeatItem parent, object key)
			{
				RepeatElement = element;
				RepeatKey = key;
				Parent = parent;
			}
		}

		public Stack<RepeatItem> ContextStack { get; set; }

		public void Push(RepeatItem item)
		{
			ContextStack.Push(item);
		}

		public RepeatItem Pop()
		{
			return ContextStack.Pop();
		}
	}
}
