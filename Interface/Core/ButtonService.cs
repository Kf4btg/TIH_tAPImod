using System;

namespace InvisibleHand
{
    public abstract class ButtonService
    {
        /// A simple, descriptive string indicating this service's function
        public abstract string ServiceType { get; }

        /// The button to which this service's
        /// actions will attach.
        protected readonly ICoreButton Client;

        public ButtonService(ICoreButton client)
        {
            this.Client = client;
        }

        /// Register required hooks with Client here
        public abstract void Subscribe();
        /// Remove registered hooks here
        public abstract void Unsubscribe();

    }
}
