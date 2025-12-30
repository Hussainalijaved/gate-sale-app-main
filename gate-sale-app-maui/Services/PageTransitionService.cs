using System;

namespace GateSale.Services
{
    public class PageTransitionService
    {
        public event Action<string, TransitionType>? OnTransitionRequested;

        public void RequestTransition(string targetRoute, TransitionType transitionType = TransitionType.SlideLeft)
        {
            OnTransitionRequested?.Invoke(targetRoute, transitionType);
        }
    }

    public enum TransitionType
    {
        None,
        Fade,
        SlideLeft,
        SlideRight,
        SlideUp,
        SlideDown
    }
}
