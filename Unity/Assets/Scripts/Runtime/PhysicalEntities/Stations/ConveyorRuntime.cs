using Gamelab.Items;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Utils;

namespace Gamelab.PhysicalEntities.Stations
{
    // Port of Src/PhysicalEntities/Stations/Conveyors/Conveyor.cs.
    // Dropped presentation: topSprite animation (animationService.SetActive), Draw
    // (base, belt top, sliding item, rotation by FacingDirection).
    // Re-declares IInteractable so OnInteract rebinds the interface slot that
    // StationRuntime's declaration owns.
    public class ConveyorRuntime : StationRuntime, IInteractable
    {
        public const float TransportDuration = 3f;

        public GridDirection FacingDirection { get; set; } = GridDirection.Right;
        public IStationGrid Grid { get; set; }
        public float TransportTimer { get; set; }

        // Settable seam for Src IsBeingHeld (AbstractGrabbable, player carrying the conveyor).
        public bool IsBeingHeld { get; set; }

        protected virtual string DefaultStationId => StationIds.Conveyor;

        public override void Initialize(StationCatalogEntry catalogEntry)
        {
            base.Initialize(catalogEntry);
            if (string.IsNullOrEmpty(StationId)) StationId = DefaultStationId;
        }

        protected virtual bool AcceptsItem(Item item) => true;

        public override bool CanReceiveItem(Item item, IItemProvider source)
        {
            if (HeldItem != null || item == null || !AcceptsItem(item)) return false;
            if (source is IPlayerActor) return true;

            return providerQueue.Count == 0 || providerQueue[0].Provider == source;
        }

        public void OnInteract(IPlayerActor interactingPlayer)
        {
            FacingDirection = FacingDirection.Clockwise();
        }

        public override Item PeekNextItem() => TransportTimer >= TransportDuration / 2f ? HeldItem : null;

        public override void Update(float dt)
        {
            base.Update(dt);
            if (IsBeingHeld) return;

            StationRuntime sourceStation = GetSourceStation();
            bool shouldPull = sourceStation != null &&
                              !(sourceStation is ConveyorRuntime sourceConveyor &&
                                sourceConveyor.FacingDirection == FacingDirection);

            StationRuntime sinkStation = GetSinkStation();
            if (sinkStation != null)
            {
                PingPullIntent(sinkStation, dt);
            }

            if (shouldPull)
            {
                Item nextItem = sourceStation.PeekNextItem();
                if (nextItem != null && AcceptsItem(nextItem))
                {
                    PingPushIntent(sourceStation, dt);
                }
            }

            if (HeldItem == null)
            {
                if (shouldPull)
                {
                    Item nextItem = sourceStation.PeekNextItem();

                    if (nextItem == null || AcceptsItem(nextItem))
                    {
                        sourceStation.PingPullIntent(this, dt);
                        if (nextItem != null && sourceStation.CanProvideItem(this) &&
                            CanReceiveItem(nextItem, sourceStation))
                        {
                            if (sourceStation.TryProvideItem(out Item grabbedItem, this))
                            {
                                ReceiveItem(grabbedItem, sourceStation);
                            }
                        }
                    }
                }
            }
            else
            {
                float halfDuration = TransportDuration / 2f;
                bool sinkReady = false;

                if (sinkStation != null)
                {
                    sinkStation.PingPushIntent(this, dt);
                    bool canReceive = sinkStation.CanReceiveItem(HeldItem, this);
                    sinkReady = canReceive && IsConsumerFirstInLine(sinkStation);
                }

                if (TransportTimer < halfDuration)
                {
                    TransportTimer += dt;

                    if (TransportTimer >= halfDuration && !sinkReady)
                    {
                        TransportTimer = halfDuration;
                    }
                }
                else if (TransportTimer < TransportDuration)
                {
                    if (sinkReady)
                    {
                        TransportTimer += dt;
                    }
                }
                else
                {
                    if (sinkReady)
                    {
                        sinkStation.ReceiveItem(HeldItem, this);
                        consumerQueue.RemoveAll(t => t.Consumer == sinkStation);
                        HeldItem = null;
                        TransportTimer = 0f;
                    }
                }
            }
        }

        public override void ReceiveItem(Item item, IItemProvider source)
        {
            HeldItem = item;
            providerQueue.RemoveAll(t => t.Provider == source);
            if (source is IPlayerActor || !ReferenceEquals(source, GetSourceStation()))
            {
                TransportTimer = TransportDuration / 2f;
            }
            else
            {
                TransportTimer = 0f;
            }
        }

        private StationRuntime GetSourceStation()
            => Grid?.GetAdjacentStation(Position, FacingDirection.Opposite());

        private StationRuntime GetSinkStation()
            => Grid?.GetAdjacentStation(Position, FacingDirection);
    }
}
