using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public class Stars : MonoBehaviour
{
	[SerializeField]
	private int MaxStars;
	[SerializeField]
	private float StarSize;
	[SerializeField]
	private float StarSizeRange;
	[SerializeField]
	private float FieldWidth;
	[SerializeField]
	private float FieldHeight;

	ParticleSystem Particles;
	ParticleSystem.Particle[] StarParticles;


	void Awake()
	{
		StarParticles = new ParticleSystem.Particle[MaxStars];
		Particles = GetComponent<ParticleSystem>();

		for (int i = 0; i < MaxStars; i++)
		{
			StarParticles[i].position = new Vector3(Random.Range(0, FieldWidth) - (FieldWidth * 0.5f), Random.Range(0, FieldHeight) - (FieldHeight * 0.5f), 0);
			float randSize = Random.Range(1f - StarSizeRange, StarSizeRange + 1f);
			StarParticles[i].startSize = StarSize * randSize;
			StarParticles[i].color = Color.white;
		}
		Particles.SetParticles(StarParticles, StarParticles.Length);
	}
}