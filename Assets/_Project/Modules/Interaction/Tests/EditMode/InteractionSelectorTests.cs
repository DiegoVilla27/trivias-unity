using NUnit.Framework;
using Trivias.Interaction.Logic;
using Trivias.Shared.Interaction;
using UnityEngine;

namespace Trivias.Interaction.Tests
{
    public sealed class InteractionSelectorTests
    {
        private sealed class FakeInteractable : IInteractable
        {
            public FakeInteractable(Vector3 position, float radius)
            {
                Position = position;
                Radius = radius;
            }

            public Vector3 Position { get; }
            public float Radius { get; }

            public bool TryGetPrompt(in InteractionContext context, out string prompt)
            {
                prompt = "test";
                return true;
            }

            public void Interact(in InteractionContext context) { }
        }

        [Test]
        public void FindClosest_ReturnsNullWhenOutOfRange()
        {
            var far = new FakeInteractable(new Vector3(5f, 0f, 0f), 1f);
            Assert.That(InteractionSelector.FindClosest(new IInteractable[] { far }, Vector3.zero), Is.Null);
        }

        [Test]
        public void FindClosest_PicksNearestInRange()
        {
            var near = new FakeInteractable(new Vector3(0.5f, 0f, 0f), 2f);
            var further = new FakeInteractable(new Vector3(1.5f, 0f, 0f), 2f);
            Assert.That(InteractionSelector.FindClosest(new IInteractable[] { further, near }, Vector3.zero), Is.SameAs(near));
        }

        [Test]
        public void FindClosest_IgnoresHeightDifference()
        {
            var raised = new FakeInteractable(new Vector3(0.5f, 3f, 0f), 1f);
            Assert.That(InteractionSelector.FindClosest(new IInteractable[] { raised }, Vector3.zero), Is.SameAs(raised));
        }
    }
}
