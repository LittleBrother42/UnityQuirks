using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SplitScreen
{
    /// <summary>
    /// Loads and plays sounds by simulating multiple listeners.
    /// </summary>
    [RequireComponent(typeof(AudioListener))]
    public class MultiListenerSoundManager : MonoBehaviour
    {
        /// <summary>
        /// The audio listener component.
        /// </summary>
        private AudioListener audioListener;

        /// <summary>
        /// The prefab to use for creating audio source objects.
        /// </summary>
        [SerializeField]
        private AudioSource audioSourcePrefab;

        /// <summary>
        /// The furthest distance from which a sound can be heard by a listener.
        /// </summary>
        [SerializeField]
        private float distanceLimit;

        /// <summary>
        /// The looping audio sources that should play constantly from a specific position in the
        /// scene.
        /// </summary>
        [SerializeField]
        private AudioSource[] environmentalAudioSources;

        /// <summary>
        /// The looping audio sources that should play constantly from a specific position in the
        /// scene, paired with the world positions at which they should play.
        /// </summary>
        private List<(AudioSource audioSource, Vector3 position)> environmentalSounds;

        /// <summary>
        /// The game objects from which sounds can be heard.
        /// </summary>
        [SerializeField]
        private GameObject[] listenerObjects;

        /// <summary>
        /// The positions and rotations of the listeners from which the sounds in this manager can
        /// be heard.
        /// </summary>
        private (Vector3 position, Quaternion rotation)[] listeners;

        /// <summary>
        /// Handles any logic specific to this behaviour that should be executed once, after the
        /// object is created and activated but before any updates have occurred.
        /// </summary>
        private void Start()
        {
            this.audioListener = this.GetComponent<AudioListener>();
            this.environmentalSounds = new List<(AudioSource audioSource, Vector3 position)>();
            this.listeners = new (Vector3 position, Quaternion rotation)[0];

            this.SetEnvironmentSoundEffects(this.environmentalAudioSources);
        }

        /// <summary>
        /// Sets the looping sound effects that play for the lifetime of this scene.
        /// </summary>
        /// <param name="audioSources">
        /// The audio source objects containing the environmental sounds.
        /// </param>
        /// <remarks>
        /// This method should be called only once per scene, and will directly modify the
        /// properties of the given audio sources.
        /// </remarks>
        private void SetEnvironmentSoundEffects(AudioSource[] audioSources)
        {
            if (audioSources == null)
            {
                // No audio sources were provided; do nothing.
                return;
            }

            // Add each audio source to the environmental audio list.
            foreach (AudioSource audioSource in audioSources)
            {
                this.environmentalSounds.Add((audioSource, audioSource.transform.position));
            }
        }

        /// <summary>
        /// Sets the true position of the specified <paramref name="audioSource"/> such that
        /// it can be heard with the volume and pan that would be expected if listened to from all
        /// <see cref="MultiListenerSoundManager.listenerPositions"/>.
        /// </summary>
        /// <param name="audioSource">
        /// The audio source to position.
        /// </param>
        /// <param name="position">
        /// The position of the sound in the game world.
        /// </param>
        private void SetSoundPosition(AudioSource audioSource, Vector3 position)
        {
            if (this.listeners.Length == 0)
            {
                // We have no listeners; do nothing.
                return;
            }

            float squareDistanceLimit = this.distanceLimit * this.distanceLimit;

            // Work out the square distance from each (virtual) listener to the sound, as well as
            // the angle from the listener to the source.
            Vector3[] differences = new Vector3[this.listeners.Length];
            Vector3 weightedDifferenceSum = Vector3.zero;
            float weightSum = 0;
            float closestSquareMagnitude = 0;
            int closestIndex = -1;
            for (int i = 0; i < this.listeners.Length; i++)
            {
                // Work out the difference between the sound and the listener. We'll start by
                // calculating the difference between positions, and then apply the rotation in the
                // opposite direction to simulate the listener itself being rotated.
                differences[i] = position - this.listeners[i].position;
                differences[i] = Quaternion.Inverse(this.listeners[i].rotation) * differences[i];

                float squareMagnitude = differences[i].sqrMagnitude;
                if (closestIndex == -1 || squareMagnitude < closestSquareMagnitude)
                {
                    closestIndex = i;
                    closestSquareMagnitude = squareMagnitude;
                }

                float weight = (squareDistanceLimit - squareMagnitude) / squareDistanceLimit;
                if (weight > 0)
                {
                    weightedDifferenceSum += differences[i] * (squareDistanceLimit - squareMagnitude);
                    weightSum += weight;
                }
            }

            // Position the sound using a magnitude based on the distance to the shortest listener,
            // at an angle derived from the weighted average of all distances.
            Vector3 weightedDifference = weightSum == 0 ? weightedDifferenceSum
                : weightedDifferenceSum / weightSum;
            if (weightedDifference.sqrMagnitude > 0)
            {
                weightedDifference.Normalize();
                weightedDifference *= Convert.ToSingle(Math.Sqrt(closestSquareMagnitude));
            }
            else if (closestSquareMagnitude > squareDistanceLimit)
            {
                // The sound is too far away; we shouldn't be able to hear it anywhere, so position
                // it an inaudible distance from the listener.
                weightedDifference = Vector3.forward * squareDistanceLimit;
            }

            audioSource.transform.position = this.audioListener.transform.position + weightedDifference;
        }

        /// <summary>
        /// Handles any logic that should be executed on each frame while the behaviour is active.
        /// </summary>
        private void Update()
        {
            // Update the listener object positions and rotations.
            this.listeners = this.listenerObjects.Select(o => (o.transform.position, o.transform.rotation)).ToArray();

            // Look through all environmental sounds, and update their positions based on the
            // positions of the listeners. Also start playing it if we aren't already; note that we
            // don't simply use "play on awake" for environmental audio, since it can lead to a few
            // ticks of play time before the listener is set up, leading to some console warnings
            // and a brief period of loud noise.
            foreach ((AudioSource audioSource, Vector3 position) environmentalAudioSource in this.environmentalSounds)
            {
                this.SetSoundPosition(environmentalAudioSource.audioSource, environmentalAudioSource.position);
                if (!environmentalAudioSource.audioSource.isPlaying)
                {
                    environmentalAudioSource.audioSource.Play();
                }
            }
        }
    }
}