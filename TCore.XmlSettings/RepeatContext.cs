using System;
using System.Collections.Generic;
using System.Text;

/*
 * Repeating items are tricky to support in a generic way.
 *
 * On read, you have to know *when* to create a new item, how to identify the item
 * you are building, and when to commit it.
 *
 * On write, you have to know if there are any remaining items to write,
 * and how to get values from the current item (and when to move to the next
 * item.
 *
 * this is achieved with a RepeatItemContext. This is the item you are building on
 * read, or querying on write.
 *
 * On read, you have to create an item at the start of the element, and commit it at
 * the end
 * On write, you have to figure out if there's more to write and identify the item
 * we are going to be writing.
 *
 * First, you have to mark the element as repeating. You mark the actual element that
 * repeats as repeating, NOT the parent element. With the following:
 * <Foos>
 *   <Foo/><Foo/><Foo/>
 * </Foos>
 *
 * <Foo> is the repeating element
 *
 * When you mark the element as repeating, you have to provide 3 delegates:
 *  AreRemainingItems - Unused on read. On Write, determine if there are more items to write
 *                      (this is easiest with an enumerator -- create one if not yet created
 *                       here, and see we can MoveNext. If so, we have the new current item,
 *                       if not, we're done)
 *
 *  CreateRepeatItemContext - On read, this creates a new item to read into. On write, this
 *                      sets the ContextItem to the value the enumerator specifies. (or if
 *                      not using an enumerator, determine what the current is)
 *
 *  CommitRepeatItem - Unused on write. On read, do the commit of the item we're building.
 *                     (add it to the collection or whatever appropriate)
 *
 * NOTES:
 *  To accommodate repeating items, you probably have to add some state variables in whatever
 *  your settings class is. Specifically, you might have to store an enumerator for the
 *  collection for write. You might also have to duplicate information that is implicitly
 *  stored (like a dictionary<string, foo> -- the key has to be read from the xml, but we
 *  don't have the rest of the object until we're done reading (maybe). so you have to store
 *  the name you are building as well.
 *
 * See the unit tests for various examples of how to do this. (NOTE that while the enumerator
 * is the simplest way of implementing collections, there is an example of manually tracking
 * the current item by index as well)
 */
namespace TCore.XmlSettings
{
    public class RepeatContext<T>
    {
        public delegate RepeatItemContext CreateRepeatItemContext(T t, Element<T> element, RepeatItemContext? parent);
        public delegate void CommitRepeatItem(T t, RepeatItemContext? itemContext);
        public delegate bool AreRemainingItems(T t, RepeatItemContext? itemContext);

        public class RepeatItemContext
        {
            public Element<T> RepeatElement { get; set; }
            public object RepeatKey { get; set; }
            public RepeatItemContext? Parent { get; set; }

            public RepeatItemContext(Element<T> element, RepeatItemContext? parent, object key)
            {
                RepeatElement = element;
                RepeatKey = key;
                Parent = parent;
            }
        }

        public Stack<RepeatItemContext> ContextStack { get; set; } = new Stack<RepeatItemContext>();

        public void Push(RepeatItemContext itemContext)
        {
            ContextStack.Push(itemContext);
        }

        public RepeatItemContext Pop()
        {
            return ContextStack.Pop();
        }
    }
}
