using System;
using System.Collections.Generic;
using System.Text;

namespace TCore.XmlSettings
{
    // Builder pattern to be used to create an XmlDescription

    // Builder.Build(Namespace, RootElementName)

    // .AddChildElement() - Add a new element as a child of the current element. This is pushed as the current element
    // .AddElement() - Add a new element as a sibling of the current element. This becomes the current element (previous element is popped)
    // .AddAttribute() - Add an attribute to the current element
    // .Pop() - Pop the current element off the stack. This is necessary to allow <Parent><Child1><Grandchild/></Child1><Child2/></Parent>
    //
    // When done building, the final result will finalize and return the completed XmlDescription

    public class XmlDescriptionBuilder<T>
    {
        private XmlDescription<T>? m_building;

        private XmlDescription<T> _Building
        {
            get
            {
                if (m_building == null)
                    throw new InvalidOperationException("must call Build() before building XmlDescription");

                return m_building;
            }
        }

        private readonly List<Element<T>> elementStack = new List<Element<T>>();

        // all of our methods return the builder. we keep internal state so we know 
        // what element to use if they use "AddElement" (which means add another
        // sibling), vs. "AddChildElement" which means to add a child element to
        // the previous element and set a new current level. 
        // Pop does what you expect -- pops up a level
        public static XmlDescriptionBuilder<T> Build(string ns, string rootElement)
        {
            XmlDescriptionBuilder<T> builder = new XmlDescriptionBuilder<T>();

            builder.m_building = new XmlDescription<T>(ns, rootElement);
            builder.elementStack.Add(builder._Building.RootElement);
            return builder;
        }

        /*----------------------------------------------------------------------------
            %%Function:Pop
            %%Qualified:TCore.XmlSettings.XmlDescriptionBuilder<T>.Pop

            Pop the current element
        ----------------------------------------------------------------------------*/
        public XmlDescriptionBuilder<T> Pop()
        {
            if (elementStack.Count == 0)
                throw new Exception("no element to add to. cannot have multiple root elements");

            elementStack.RemoveAt(elementStack.Count - 1);
            return this;
        }

        /*----------------------------------------------------------------------------
            %%Function:TerminateAfterReadingElement
            %%Qualified:TCore.XmlSettings.XmlDescriptionBuilder<T>.TerminateAfterReadingElement

            After completely reading the element (including its values or children),
            cancel the parse gracefully
        ----------------------------------------------------------------------------*/
        public XmlDescriptionBuilder<T> TerminateAfterReadingElement()
        {
            if (elementStack.Count == 0)
                throw new Exception("no element to add to. cannot have multiple root elements");

            elementStack[elementStack.Count - 1].TerminateAfterReadingElement = true;
            return this;
        }

        /*----------------------------------------------------------------------------
            %%Function:TerminateAfterReadingAttributes
            %%Qualified:TCore.XmlSettings.XmlDescriptionBuilder<T>.TerminateAfterReadingAttributes

            After reading the attributes for this element (but none of its children
            or its value), cancel the parse gracefully
        ----------------------------------------------------------------------------*/
        public XmlDescriptionBuilder<T> TerminateAfterReadingAttributes()
        {
            if (elementStack.Count == 0)
                throw new Exception("no element to add to. cannot have multiple root elements");

            elementStack[elementStack.Count - 1].TerminateAfterReadingAttributes = true;
            return this;
        }

        /*----------------------------------------------------------------------------
            %%Function:DiscardAttributesWithNoSetter
            %%Qualified:TCore.XmlSettings.XmlDescriptionBuilder<T>.DiscardAttributesWithNoSetter

            If we have a defined attribute, but not setter provided, just ignore
            the attribute
        ----------------------------------------------------------------------------*/
        public XmlDescriptionBuilder<T> DiscardAttributesWithNoSetter()
        {
            _Building.DiscardAttributesWithNoSetter = true;
            return this;
        }

        /*----------------------------------------------------------------------------
            %%Function:DiscardUnknownAttributes
            %%Qualified:TCore.XmlSettings.XmlDescriptionBuilder<T>.DiscardUnknownAttributes

            if we encounter an attribute we don't recognize, just skip it
        ----------------------------------------------------------------------------*/
        public XmlDescriptionBuilder<T> DiscardUnknownAttributes()
        {
            _Building.DiscardUnknownAttributes = true;
            return this;
        }

        /*----------------------------------------------------------------------------
            %%Function:AddChildElement
            %%Qualified:TCore.XmlSettings.XmlDescriptionBuilder<T>.AddChildElement

            Add a new element as a child of the current element
        ----------------------------------------------------------------------------*/
        public XmlDescriptionBuilder<T> AddChildElement(
            string elementName,
            Element<T>.GetValueDelegate? getValueDelegate = null,
            Element<T>.SetValueDelegate? setValueDelegate = null,
            string? ns = null)
        {
            Element<T> element = new Element<T>(_Building, ns ?? _Building.Namespace, elementName, getValueDelegate, setValueDelegate, null);

            // add this element as a child of the current element
            elementStack[elementStack.Count - 1].Children.Add(element);

            // and push the current element as the new current element
            elementStack.Add(element);

            return this;
        }

        /*----------------------------------------------------------------------------
            %%Function:AddElement
            %%Qualified:TCore.XmlSettings.XmlDescriptionBuilder.AddElement

            This adds this new element as a sibling to the current element.
            (Specifically, this pops the current element, then adds the element as 
            a child of the new current element)
        ----------------------------------------------------------------------------*/
        public XmlDescriptionBuilder<T> AddElement(
            string elementName,
            Element<T>.GetValueDelegate? getValueDelegate = null,
            Element<T>.SetValueDelegate? setValueDelegate = null,
            string? ns = null)
        {
            return Pop().AddChildElement(elementName, getValueDelegate, setValueDelegate, ns);
        }

        /*----------------------------------------------------------------------------
            %%Function:SetRepeating
            %%Qualified:TCore.XmlSettings.XmlDescriptionBuilder<T>.SetRepeating

            Set the current element as a repeating element. Must provide the 
            3 delegates.
        ----------------------------------------------------------------------------*/
        public XmlDescriptionBuilder<T> SetRepeating(
            RepeatContext<T>.CreateRepeatItemContext createRepeatItemContextDelegate,
            RepeatContext<T>.AreRemainingItems areRemainingItemsDelegate,
            RepeatContext<T>.CommitRepeatItem commitRepeatItemDelegate)
        {
            elementStack[elementStack.Count - 1].SetRepeating(createRepeatItemContextDelegate, areRemainingItemsDelegate, commitRepeatItemDelegate);
            return this;
        }

        /*----------------------------------------------------------------------------
            %%Function:AddAttribute
            %%Qualified:TCore.XmlSettings.XmlDescriptionBuilder<T>.AddAttribute

        ----------------------------------------------------------------------------*/
        public XmlDescriptionBuilder<T> AddAttribute(
            string attributeName,
            Attribute<T>.GetValueDelegate? getValueDelegate = null,
            Attribute<T>.SetValueDelegate? setValueDelegate = null,
            string? ns = null)
        {
            elementStack[elementStack.Count - 1].Attributes?.Add(new Attribute<T>(ns, attributeName, getValueDelegate, setValueDelegate));
            return this;
        }

        /*----------------------------------------------------------------------------
            %%Function:implicit operator XmlDescription<T>
            %%Qualified:TCore.XmlSettings.XmlDescriptionBuilder<T>.implicit operator TCore.XmlSettings.XmlDescription<T>

            implicitly retrieve the final XmlDescription
        ----------------------------------------------------------------------------*/
        public static implicit operator XmlDescription<T>(XmlDescriptionBuilder<T> builder)
        {
            return builder._Building;
        }
    }
}
