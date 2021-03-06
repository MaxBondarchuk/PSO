﻿using System;

namespace PSO
{
	public class Algorithm
	{
		#region Constants: private

		private const double PhiP = 1;
		private const double PhiG = 1;

		#endregion

		#region Properties: private

		private Swarm Swarm { get; } = new Swarm();
		private Function Func { get; set; }

		private Random Random { get; } = new Random();

		#endregion

		private void Initialize(int swarmSize, Function func)
		{
			Func = func;

			Swarm.Particles.Clear();
			for (int i = 0; i < swarmSize; i++)
			{
				var particle = new Particle();
				particle.Generate(Func, Random);
				Swarm.Particles.Add(particle);

				if (particle.Fp < Swarm.Fg)
				{
					Swarm.UpdateG(particle.P, Func);
				}
			}
		}

		public void Run(int swarmSize, Function func)
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();

			Initialize(swarmSize, func);

			double vMax = Math.Abs(Func.BoundUpper - Func.BoundLower) * .1;
			const double wLow = 0.1;

			var best = new Particle();

			int lastImprovementOn = 0;
			for (int iter = 0; iter < func.IterationsNumber; iter++)
			{
				double w = func.MaxVelocity - (func.MaxVelocity - wLow) * iter / func.IterationsNumber;

				foreach (Particle particle in Swarm.Particles)
				{
					for (int d = 0; d < Func.Dimensions; d++)
					{
						double rp = Random.NextDouble();
						double rg = Random.NextDouble();

						particle.V[d] = w * particle.V[d] + PhiP * rp * (particle.P[d] - particle.X[d]) +
						                PhiG * rg * (Swarm.G[d] - particle.X[d]);

						particle.V[d] = Math.Min(particle.V[d], vMax);
						particle.V[d] = Math.Max(particle.V[d], -vMax);

						particle.X[d] += particle.V[d];

						particle.X[d] = Math.Max(particle.X[d], Func.BoundLower);
						particle.X[d] = Math.Min(particle.X[d], Func.BoundUpper);
					}

					particle.Fx = Func.F(particle.X);
					if (particle.Fx < particle.Fp)
					{
						particle.UpdateP();
					}
					else if (Random.NextDouble() <= func.KillProbability)
					{
						particle.Generate(func, Random);
					}

					if (particle.Fp < Swarm.Fg)
					{
						Swarm.UpdateG(particle.P, Func);
						lastImprovementOn = iter;
						best = particle;
					}
				}


				Console.WriteLine($"#{iter,-4} Best G = {Swarm.Fg,-16:0.000000000000} W = {w,-7:0.00000}");
			}

			watch.Stop();
			Console.WriteLine($"\nLast improvement was on iteration #{lastImprovementOn}. Time elapsed: {watch.Elapsed}");
			Console.WriteLine("Coordinates of the best");
			foreach (double x in best.X)
			{
				Console.Write($"{x,20}");
			}

			Console.WriteLine();
		}
	}
}