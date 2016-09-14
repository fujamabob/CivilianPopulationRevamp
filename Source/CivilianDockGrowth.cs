using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP;

namespace CivilianPopulationRevamp
{
    public class CivilianDockGrowth : CivilianPopulationRegulator
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

                TimeUntilGrowth -= dt * getRecruitmentSoIModifier();
                if (TimeUntilGrowth <= 0.0)
                {
                    placeNewCivilian();
                    TimeUntilGrowth = TimeBetweenGrowth;
                }
            }
        }

        /// <summary>
        /// Gets the recruitment modifier from being with Kerbin's, Mun's. or Minmus' SoI.  It is easier for competing
        /// programs to get astronauts into Kerbin orbit than it is to Mun/Minus.  None of them are as good as you are.
        /// </summary>
        /// <returns>The recruitment modifier due to.</returns>
        double getRecruitmentSoIModifier()
        {
            if (vessel.LandedOrSplashed)
              return 0.0;

            double recruitmentRateModifier;
            switch (FlightGlobals.currentMainBody.name)
            {
                case "Kerbin":
                    recruitmentRateModifier = 1.0;
                    break;
                case "Mun":
                    recruitmentRateModifier = 0.5;
                    break;
                case "Minmus":
                    recruitmentRateModifier = 0.25;
                    break;
                default:
                    recruitmentRateModifier = 0;
                    break;
            }
            return recruitmentRateModifier;
        }
    }
}