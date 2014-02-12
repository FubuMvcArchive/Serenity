using System;
using System.Collections.Generic;
using System.Linq;
using FubuTestingSupport;
using NUnit.Framework;
using OpenQA.Selenium;
using Rhino.Mocks;
using Serenity.Fixtures.Handlers;

namespace Serenity.Testing.Fixtures.Handlers
{
    public class ElementHandlerWrapperTester : InteractionContext<ElementHandlerWrapperTester.TestElementHandlerWrapper>
    {
        private const string Result = "Some returned text";

        private IWebElement _element;
        private ISearchContext _context;
        private object _data;

        protected override void beforeEach()
        {
            _element = MockFor<IWebElement>();
            _context = MockFor<ISearchContext>();
            _data = new object();

            ClassUnderTest.Element = _element;
            ClassUnderTest.Context = _context;
            ClassUnderTest.Data = _data;
        }

        [Test]
        public void DoesNotCallNestedMatchesIfWrapperDoesNotMatch()
        {
            ElementHandlerWrapper.AllHandlers = () =>
            {
                throw new Exception("Should not attempt to get all handlers");
            };

            ClassUnderTest.MatchesResult = false;

            ClassUnderTest.Matches(_element).ShouldBeFalse();
        }

        [Test]
        public void DoesNotMatchIfThereAreNoMatchingInnerHandlers()
        {
            var handlers = new List<IElementHandler>
            {
                ClassUnderTest,
                MockedHandler(false),
                MockedHandler(false)
            };

            ElementHandlerWrapper.AllHandlers = () => handlers;

            ClassUnderTest.Matches(_element).ShouldBeFalse();
            handlers[1].AssertWasCalled(x => x.Matches(_element));
            handlers[2].AssertWasCalled(x => x.Matches(_element));
        }

        [Test]
        public void MatchesOnlyConsidersHandlersAfterItself()
        {
            var handlers = new List<IElementHandler>
            {
                MockedHandler(true),
                ClassUnderTest,
                MockedHandler(false)
            };

            ElementHandlerWrapper.AllHandlers = () => handlers;

            ClassUnderTest.Matches(_element).ShouldBeFalse();
            handlers[0].AssertWasNotCalled(x => x.Matches(_element));
            handlers[2].AssertWasCalled(x => x.Matches(_element));
        }

        [Test]
        public void Matches()
        {
            var handlers = new List<IElementHandler>
            {
                ClassUnderTest,
                MockedHandler(false),
                MockedHandler(true),
                MockedHandler(false)
            };

            ElementHandlerWrapper.AllHandlers = () => handlers;

            ClassUnderTest.Matches(_element).ShouldBeTrue();
            handlers[1].AssertWasCalled(x => x.Matches(_element));
            handlers[2].AssertWasCalled(x => x.Matches(_element));
            handlers[3].AssertWasNotCalled(x => x.Matches(_element));
        }

        [Test]
        public void EnterDataOrderOfCalls()
        {
            var handlers = new List<IElementHandler>
            {
                ClassUnderTest,
                MockedHandler(false),
                MockedHandler(true),
                MockedHandler(false)
            };

            ElementHandlerWrapper.AllHandlers = () => handlers;

            ClassUnderTest.EnterBefore = () =>
            {
                handlers.Skip(1).Each(h => h.AssertWasNotCalled(x => x.Matches(_element)));
                handlers.Skip(1).Each(h => h.AssertWasNotCalled(x => x.EnterData(_context, _element, _data)));
            };

            ClassUnderTest.EnterAfter = () =>
            {
                handlers[1].AssertWasCalled(x => x.Matches(_element));
                handlers[2].AssertWasCalled(x => x.Matches(_element));
                handlers[3].AssertWasNotCalled(x => x.Matches(_element));

                handlers[1].AssertWasNotCalled(x => x.EnterData(_context, _element, _data));
                handlers[2].AssertWasCalled(x => x.EnterData(_context, _element, _data));
                handlers[3].AssertWasNotCalled(x => x.EnterData(_context, _element, _data));
            };

            ClassUnderTest.EnterData(_context, _element, _data);

            ClassUnderTest.AssertEnterDataCalled(true);
            ClassUnderTest.AssertGetDataCalled(false);
        }

        [Test]
        public void GetDataOrderOfCalls()
        {
            var handlers = new List<IElementHandler>
            {
                ClassUnderTest,
                MockedHandler(false),
                MockedHandler(true),
                MockedHandler(false)
            };

            ElementHandlerWrapper.AllHandlers = () => handlers;

            ClassUnderTest.EnterBefore = () =>
            {
                handlers.Skip(1).Each(h => h.AssertWasNotCalled(x => x.Matches(_element)));
                handlers.Skip(1).Each(h => h.AssertWasNotCalled(x => x.GetData(_context, _element)));
            };

            ClassUnderTest.EnterAfter = () =>
            {
                handlers[1].AssertWasCalled(x => x.Matches(_element));
                handlers[2].AssertWasCalled(x => x.Matches(_element));
                handlers[3].AssertWasNotCalled(x => x.Matches(_element));

                handlers[1].AssertWasNotCalled(x => x.GetData(_context, _element));
                handlers[2].AssertWasCalled(x => x.GetData(_context, _element));
                handlers[3].AssertWasNotCalled(x => x.GetData(_context, _element));
            };

            ClassUnderTest.GetData(_context, _element).ShouldEqual(Result);

            ClassUnderTest.AssertEnterDataCalled(false);
            ClassUnderTest.AssertGetDataCalled(true);
        }

        private IElementHandler MockedHandler(bool matches)
        {
            var handler = MockedHandler();
            handler.Stub(x => x.Matches(_element)).Return(matches);

            if (matches)
            {
                handler.Stub(x => x.GetData(_context, _element)).Return(Result);
            }

            return handler;
        }

        private IElementHandler MockedHandler()
        {
            return Services.AddAdditionalMockFor<IElementHandler>();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            ElementHandlerWrapper.ResetAllHandlers();
        }

        public class TestElementHandlerWrapper : ElementHandlerWrapper
        {
            public bool MatchesResult { get; set; }

            public Action EnterBefore { get; set; }
            public Action EnterAfter { get; set; }

            public Action GetBefore { get; set; }
            public Action GetAfter { get; set; }

            public IWebElement Element { get; set; }
            public ISearchContext Context { get; set; }
            public object Data { get; set; }

            private bool _calledBeforeEnter;
            private bool _calledAfterEnter;
            private bool _calledBeforeGet;
            private bool _calledAfterGet;

            public TestElementHandlerWrapper()
            {
                MatchesResult = true;
                EnterBefore = () => { };
                EnterAfter = () => { };
                GetBefore = () => { };
                GetAfter = () => { };
            }

            protected override bool WrapperMatches(IWebElement element)
            {
                ReferenceEquals(Element, element).ShouldBeTrue();
                return MatchesResult;
            }

            protected override void EnterDataBeforeNested(ISearchContext context, IWebElement element, object data)
            {
                _calledBeforeEnter = true;
                ReferenceEquals(Context, context).ShouldBeTrue();
                ReferenceEquals(Element, element).ShouldBeTrue();
                ReferenceEquals(Data, data).ShouldBeTrue();
                EnterBefore();
            }

            protected override void EnterDataAfterNested(ISearchContext context, IWebElement element, object data)
            {
                _calledAfterEnter = true;
                ReferenceEquals(Context, context).ShouldBeTrue();
                ReferenceEquals(Element, element).ShouldBeTrue();
                ReferenceEquals(Data, data).ShouldBeTrue();
                EnterAfter();
            }

            protected override void GetDataBeforeNested(ISearchContext context, IWebElement element)
            {
                _calledBeforeGet = true;
                ReferenceEquals(Context, context).ShouldBeTrue();
                ReferenceEquals(Element, element).ShouldBeTrue();
                GetBefore();
            }

            protected override void GetDataAfterNested(ISearchContext context, IWebElement element)
            {
                _calledAfterGet = true;
                ReferenceEquals(Context, context).ShouldBeTrue();
                ReferenceEquals(Element, element).ShouldBeTrue();
                GetAfter();
            }

            public void AssertEnterDataCalled(bool called)
            {
                _calledBeforeEnter.ShouldEqual(called);
                _calledAfterEnter.ShouldEqual(called);
            }

            public void AssertGetDataCalled(bool called)
            {
                _calledBeforeGet.ShouldEqual(called);
                _calledAfterGet.ShouldEqual(called);
            }
        }
    }
}