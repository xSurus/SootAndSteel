// Unity/Assets/Scripts/Runtime/PhysicalEntities/Stations/StationRuntime.cs
using System.Collections.Generic;
using Gamelab.Items;
using Gamelab.PhysicalEntities.Interfaces;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Stations
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class StationRuntime : MonoBehaviour, IInteractable, IPickable, IItemProvider, IItemReceiver, IUpdatable
    {
        private const float TimeUntilKick = 0.5f;

        public string StationId { get; protected set; }
        public Item HeldItem { get; set; }
        public Rigidbody2D PhysicsBody { get; private set; }

        public Vector2 Position
        {
            get => PhysicsBody != null ? PhysicsBody.position : (Vector2)transform.position;
            set
            {
                if (PhysicsBody != null) PhysicsBody.position = value;
                else transform.position = value;
            }
        }

        protected readonly List<ProviderTicket> providerQueue = new List<ProviderTicket>();
        protected readonly List<ConsumerTicket> consumerQueue = new List<ConsumerTicket>();

        public virtual void Initialize(StationCatalogEntry catalogEntry)
        {
            StationId = catalogEntry.stationId;
            PhysicsBody = GetComponent<Rigidbody2D>();
            PhysicsBody.bodyType = RigidbodyType2D.Static;
        }

        public virtual Item PeekNextItem() => HeldItem;

        public virtual bool CanReceiveItem(Item item, IItemProvider source) => false;

        public virtual bool TryProvideItem(out Item item, IItemReceiver consumer = null)
        {
            item = HeldItem;
            if (HeldItem != null && CanProvideItem(consumer))
            {
                HeldItem = null;
                if (consumer != null)
                {
                    consumerQueue.RemoveAll(t => t.Consumer == consumer);
                }

                return true;
            }

            item = null;
            return false;
        }

        public virtual void ReceiveItem(Item item, IItemProvider source)
        {
            HeldItem = item;
        }

        public virtual bool CanProvideItem(IItemReceiver consumer)
        {
            return HeldItem != null && IsConsumerFirstInLine(consumer);
        }

        public void PingPushIntent(IItemProvider source, float dt)
        {
            ProviderTicket existing = providerQueue.Find(t => t.Provider == source);
            if (existing != null)
            {
                existing.TimeSinceLastPing = 0f;
                return;
            }

            providerQueue.Add(new ProviderTicket(source));
        }

        public void PingPullIntent(IItemReceiver consumer, float dt)
        {
            if (consumer == null)
            {
                return;
            }

            ConsumerTicket existing = consumerQueue.Find(t => t.Consumer == consumer);
            if (existing != null)
            {
                existing.TimeSinceLastPing = 0f;
                return;
            }

            consumerQueue.Add(new ConsumerTicket(consumer));
        }

        protected bool IsConsumerFirstInLine(IItemReceiver consumer)
        {
            // Src AbstractStation: a null consumer (default-arg TryProvideItem) and a
            // Player consumer both always pass the FIFO check.
            if (consumer == null || consumer is IPlayerActor)
            {
                return true;
            }

            return consumerQueue.Count == 0 || consumerQueue[0].Consumer == consumer;
        }

        // Unity message; subclasses override Update(float), never this.
        private void Update() => Update(Time.deltaTime);

        public virtual void Update(float dt)
        {
            for (int i = providerQueue.Count - 1; i >= 0; i--)
            {
                providerQueue[i].TimeSinceLastPing += dt;
                if (providerQueue[i].TimeSinceLastPing > TimeUntilKick)
                {
                    providerQueue.RemoveAt(i);
                }
            }

            for (int i = consumerQueue.Count - 1; i >= 0; i--)
            {
                consumerQueue[i].TimeSinceLastPing += dt;
                if (consumerQueue[i].TimeSinceLastPing > TimeUntilKick)
                {
                    consumerQueue.RemoveAt(i);
                }
            }
        }
    }
}
