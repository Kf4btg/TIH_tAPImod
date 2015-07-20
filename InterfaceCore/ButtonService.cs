using System;

namespace InvisibleHand
{
    public abstract class ButtonService
    {
        /// A short string indicating this service's function
        public abstract string ServiceType { get; }

        /// The button to which this service's
        /// actions will attach.
        protected readonly CoreButton Client;

        public ButtonService(CoreButton client)
        {
            this.Client = client;
        }

        /// Register required hooks with Client here
        public abstract void Subscribe();
        /// Remove registered hooks here
        public abstract void Unsubscribe();

    }
}
