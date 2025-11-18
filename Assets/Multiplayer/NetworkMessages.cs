using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Network message structures for deterministic peer-to-peer gameplay.
/// These messages ensure both clients execute the same actions with identical parameters.
/// </summary>
namespace GravityWars.Multiplayer
{
    #region Player Input Messages

    /// <summary>
    /// Player rotation input - sent every frame when rotating
    /// </summary>
    public struct PlayerRotationInput : INetworkSerializable
    {
        public ulong playerId;
        public float rotationDelta;  // Rotation change this frame
        public uint tick;            // Game tick for synchronization

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerId);
            serializer.SerializeValue(ref rotationDelta);
            serializer.SerializeValue(ref tick);
        }
    }

    /// <summary>
    /// Player thrust input - sent every frame when thrusting
    /// </summary>
    public struct PlayerThrustInput : INetworkSerializable
    {
        public ulong playerId;
        public float thrustAmount;   // 0-1 thrust intensity
        public uint tick;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerId);
            serializer.SerializeValue(ref thrustAmount);
            serializer.SerializeValue(ref tick);
        }
    }

    /// <summary>
    /// Player fire action - CRITICAL for determinism
    /// Contains exact parameters so both clients spawn identical missile
    /// </summary>
    public struct PlayerFireAction : INetworkSerializable
    {
        public ulong playerId;
        public Vector3 spawnPosition;
        public Quaternion spawnRotation;
        public Vector3 initialVelocity;
        public float fireAngle;          // Ship angle at time of fire
        public float firePower;          // Power multiplier
        public uint tick;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerId);
            serializer.SerializeValue(ref spawnPosition);
            serializer.SerializeValue(ref spawnRotation);
            serializer.SerializeValue(ref initialVelocity);
            serializer.SerializeValue(ref fireAngle);
            serializer.SerializeValue(ref firePower);
            serializer.SerializeValue(ref tick);
        }
    }

    /// <summary>
    /// Player move action (for precision/warp moves)
    /// </summary>
    public struct PlayerMoveAction : INetworkSerializable
    {
        public ulong playerId;
        public Vector3 targetPosition;
        public int moveType;  // 0=Regular, 1=Precision, 2=Warp
        public uint tick;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerId);
            serializer.SerializeValue(ref targetPosition);
            serializer.SerializeValue(ref moveType);
            serializer.SerializeValue(ref tick);
        }
    }

    #endregion

    #region Turn State Messages

    /// <summary>
    /// Turn phase change - server controls game flow
    /// </summary>
    public struct TurnStateChange : INetworkSerializable
    {
        public enum Phase
        {
            GameStart,
            PreparationPhase,
            PlayerTurn,
            MissileFlight,
            RoundEnd,
            GameOver
        }

        public Phase phase;
        public ulong currentPlayerId;
        public float phaseDuration;  // Timer duration in seconds
        public int roundNumber;
        public uint tick;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref phase);
            serializer.SerializeValue(ref currentPlayerId);
            serializer.SerializeValue(ref phaseDuration);
            serializer.SerializeValue(ref roundNumber);
            serializer.SerializeValue(ref tick);
        }
    }

    /// <summary>
    /// Timer update - keeps clients synchronized
    /// </summary>
    public struct TimerUpdate : INetworkSerializable
    {
        public float timeRemaining;
        public uint tick;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref timeRemaining);
            serializer.SerializeValue(ref tick);
        }
    }

    #endregion

    #region Game Events

    /// <summary>
    /// Damage event - server validates and broadcasts
    /// </summary>
    public struct DamageEvent : INetworkSerializable
    {
        public ulong targetPlayerId;
        public ulong sourcePlayerId;
        public float damage;
        public Vector3 hitPosition;
        public bool wasCritical;
        public uint tick;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref targetPlayerId);
            serializer.SerializeValue(ref sourcePlayerId);
            serializer.SerializeValue(ref damage);
            serializer.SerializeValue(ref hitPosition);
            serializer.SerializeValue(ref wasCritical);
            serializer.SerializeValue(ref tick);
        }
    }

    /// <summary>
    /// Missile destroyed event - clients report, server validates
    /// </summary>
    public struct MissileDestroyedEvent : INetworkSerializable
    {
        public enum DestructionReason
        {
            HitPlanet,
            HitShip,
            OutOfBounds,
            OutOfFuel,
            SelfDestruct
        }

        public ulong ownerPlayerId;
        public int missileId;  // Local missile ID
        public DestructionReason reason;
        public Vector3 finalPosition;
        public uint tick;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ownerPlayerId);
            serializer.SerializeValue(ref missileId);
            serializer.SerializeValue(ref reason);
            serializer.SerializeValue(ref finalPosition);
            serializer.SerializeValue(ref tick);
        }
    }

    /// <summary>
    /// Ship destroyed event
    /// </summary>
    public struct ShipDestroyedEvent : INetworkSerializable
    {
        public ulong destroyedPlayerId;
        public ulong killerPlayerId;
        public Vector3 deathPosition;
        public uint tick;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref destroyedPlayerId);
            serializer.SerializeValue(ref killerPlayerId);
            serializer.SerializeValue(ref deathPosition);
            serializer.SerializeValue(ref tick);
        }
    }

    #endregion

    #region Match Setup Messages

    /// <summary>
    /// Match initialization - ensures deterministic setup
    /// </summary>
    public struct MatchInitData : INetworkSerializable
    {
        public int randomSeed;  // For deterministic planet spawning
        public ulong player1Id;
        public ulong player2Id;
        public Vector3 player1SpawnPos;
        public Vector3 player2SpawnPos;
        public Quaternion player1SpawnRot;
        public Quaternion player2SpawnRot;
        public int winningScore;  // Best of 3/5/7

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref randomSeed);
            serializer.SerializeValue(ref player1Id);
            serializer.SerializeValue(ref player2Id);
            serializer.SerializeValue(ref player1SpawnPos);
            serializer.SerializeValue(ref player2SpawnPos);
            serializer.SerializeValue(ref player1SpawnRot);
            serializer.SerializeValue(ref player2SpawnRot);
            serializer.SerializeValue(ref winningScore);
        }
    }

    /// <summary>
    /// Planet spawn data for deterministic generation
    /// </summary>
    public struct PlanetSpawnData : INetworkSerializable
    {
        public Vector3 position;
        public Quaternion rotation;
        public float mass;
        public int prefabIndex;  // Index into planetInfos array

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref mass);
            serializer.SerializeValue(ref prefabIndex);
        }
    }

    #endregion

    #region Score & UI Messages

    /// <summary>
    /// Score update
    /// </summary>
    public struct ScoreUpdate : INetworkSerializable
    {
        public ulong playerId;
        public int newScore;
        public uint tick;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerId);
            serializer.SerializeValue(ref newScore);
            serializer.SerializeValue(ref tick);
        }
    }

    #endregion
}
