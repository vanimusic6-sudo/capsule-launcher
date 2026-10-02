using NUnit.Framework;
using Playnite.Controllers;
using Playnite.FullscreenApp.Capsule.Experience;
using System;

namespace Playnite.FullscreenApp.Tests.Capsule.Experience
{
    [TestFixture]
    public class CapsuleGameSessionStateTests
    {
        [Test]
        public void LaunchLifecycleSuspendsPresentationUntilReturnCompletes()
        {
            var state = new CapsuleGameSessionState(new GameControllerFactory());
            var gameId = Guid.NewGuid();

            state.MarkStarting(gameId);
            Assert.AreEqual(CapsuleGameSessionPhase.Starting, state.Phase);
            Assert.IsTrue(state.IsPresentationSuspended);
            Assert.AreEqual(gameId, state.PrimaryGameId);

            state.MarkRunning(gameId);
            Assert.AreEqual(CapsuleGameSessionPhase.Running, state.Phase);
            Assert.IsTrue(state.IsPresentationSuspended);

            state.MarkStopped(gameId);
            Assert.AreEqual(CapsuleGameSessionPhase.Returning, state.Phase);
            Assert.IsTrue(state.IsPresentationSuspended);

            state.CompleteReturn();
            Assert.AreEqual(CapsuleGameSessionPhase.Idle, state.Phase);
            Assert.IsFalse(state.IsPresentationSuspended);
            Assert.IsNull(state.PrimaryGameId);
        }

        [Test]
        public void RemainingGameKeepsPresentationSuspended()
        {
            var state = new CapsuleGameSessionState(new GameControllerFactory());
            var first = Guid.NewGuid();
            var second = Guid.NewGuid();

            state.MarkStarting(first);
            state.MarkRunning(first);
            state.MarkStarting(second);
            state.MarkRunning(second);

            state.MarkStopped(first);

            Assert.AreEqual(CapsuleGameSessionPhase.Running, state.Phase);
            Assert.IsTrue(state.IsPresentationSuspended);
            Assert.AreEqual(1, state.ActiveGameCount);
        }

        [Test]
        public void CancelledStartupUsesSameReturnPhase()
        {
            var state = new CapsuleGameSessionState(new GameControllerFactory());
            var gameId = Guid.NewGuid();

            state.MarkStarting(gameId);
            state.MarkStartupCancelled(gameId);

            Assert.AreEqual(CapsuleGameSessionPhase.Returning, state.Phase);
            Assert.IsTrue(state.IsPresentationSuspended);

            state.CompleteReturn();
            Assert.AreEqual(CapsuleGameSessionPhase.Idle, state.Phase);
        }
    }
}
