using System;
using System.Collections.Generic;
using UnityEngine;
using KSP;

namespace CivilianPopulationRevamp
{
    public class CivilianSpawnGrowth : CivilianPopulationRegulator
    {
        public void Update()
        {
            double total_dt = GetDeltaTime();
            while (total_dt > 0.0)
            {
                // Replay events in case of a long wait
                double dt = calculate_timestep(total_dt, TimeUntilGrowth, TimeUntilTaxes);
                total_dt -= dt;

                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                {
                    TimeUntilTaxes -= dt;
                    int civilianPopulation = countCivilians();
                    if (TimeUntilTaxes <= 0.0)
                    {
                        getTaxes(civilianPopulation);
                        TimeUntilTaxes = TimeBetweenTaxes;
                    }
                }

                TimeUntilGrowth -= dt;
                if (TimeUntilGrowth <= 0.0)
                {
                    placeNewCivilian();
                    TimeUntilGrowth = TimeBetweenGrowth;
                }
            }
        }
    }
}

