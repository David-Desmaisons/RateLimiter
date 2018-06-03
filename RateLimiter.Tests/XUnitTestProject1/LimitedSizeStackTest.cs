using FluentAssertions;
using Xunit;

namespace RateLimiter.Tests
{
    public class LimitedSizeStackTest
    {
        private readonly LimitedSizeStack<int> _LimitedSizeStack;
        private readonly LimitedSizeStack<int> _LimitedSizeStackFull;
        public LimitedSizeStackTest()
        {
            _LimitedSizeStack = new LimitedSizeStack<int>(5);
            _LimitedSizeStackFull = new LimitedSizeStack<int>(5);
            for ( int i=0; i<5; i++)
            {
                _LimitedSizeStackFull.AddFirst(i);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        public void Push_AddElement_InFirstPosition(int element)
        {
            _LimitedSizeStack.Push(element);
            _LimitedSizeStack.First.Value.Should().Be(element);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        public void Push_AddElementOnFullColection_InFirstPosition(int element)
        {
            _LimitedSizeStackFull.Push(element);
            _LimitedSizeStackFull.First.Value.Should().Be(element);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        public void Push_WhenCollectionHasFullSize_MaintainsNumberOfElement(int element)
        {
            _LimitedSizeStackFull.Push(element);
            _LimitedSizeStackFull.Count.Should().Be(5);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        public void Push_WhenCollectionHasFullSize_RemoveLastElement(int element)
        {
            _LimitedSizeStackFull.Push(element);
            _LimitedSizeStackFull.Last.Value.Should().Be(1);
        }
    }
}
