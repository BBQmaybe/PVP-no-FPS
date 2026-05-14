using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Unity.Template.CompetitiveActionMultiplayer
{
    [GhostComponent]
    public struct FirstPersonCharacterComponent : IComponentData
    {
        public float BaseFov;
        public float GroundMaxSpeed;
        public float GroundedMovementSharpness;
        public float AirAcceleration;
        public float AirMaxSpeed;
        public float AirDrag;
        public float JumpSpeed;
        public float3 Gravity;
        public bool PreventAirAccelerationAgainstUngroundedHits;
        public BasicStepAndSlopeHandlingParameters StepAndSlopeHandling;

        public float MinViewAngle;
        public float MaxViewAngle;
        public float ViewRollAmount;
        public float ViewRollSharpness;

        public Entity ViewEntity;

        public Entity NameTagSocketEntity;
        public Entity WeaponSocketEntity;
        public Entity WeaponAnimationSocketEntity;

        public Entity DeathVfxSpawnPoint;

        [GhostField(Quantization = 1000, Smoothing = SmoothingAction.InterpolateAndExtrapolate)]
        public float CharacterYDegrees;

        [GhostField(Quantization = 1000, Smoothing = SmoothingAction.InterpolateAndExtrapolate)]
        public float ViewPitchDegrees;

        public quaternion ViewLocalRotation;
        public float ViewRollDegrees;
        public byte HasProcessedDeath;

        public byte MaxJumpCount;
        public byte JumpCount;

        public float SlowFallGravityScale;

        public float DashSpeed;
        public float DashDuration;
        public float DashCooldown;
        public float DashTimer;
        public float DashCooldownTimer;
        public float3 DashDirection;

        public float SprintSpeedMultiplier;
        public float SprintDuration;
        public float SprintTimer;
        public float SprintCooldown;
        public float SprintCooldownTimer;
    }

    public struct CharacterInitialized : IComponentData, IEnableableComponent
    {
    }

    public struct FirstPersonCharacterControl : IComponentData
    {
        public float3 MoveVector;
        public float2 LookYawPitchDegreesDelta;
        public bool Jump;
        public bool SlowFall;
        public bool Dash;
        public bool SprintHeld;
    }

    public struct FirstPersonCharacterView : IComponentData
    {
        public Entity CharacterEntity;
    }

    [GhostComponent]
    public struct OwningPlayer : IComponentData
    {
        [GhostField]
        public Entity Entity;
    }
}
