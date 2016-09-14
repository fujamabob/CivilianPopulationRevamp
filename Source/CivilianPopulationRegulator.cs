using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP;

namespace CivilianPopulationRevamp
{
    public class CivilianPopulationRegulator : PartModule
    {
        /// <summary>
        /// The time until a new crew member is added
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false)]
        public double TimeBetweenGrowth;

        /// <summary>
        /// The time until a new crew member is added
        /// </summary>
        [KSPField(isPersistant = true, guiActive = true,
                  guiName = "Time to new crew", guiFormat = "F2")]
        public double TimeUntilGrowth = -1d;

        /// <summary>
        /// The time until a new crew member is added
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false)]
        public double TimeBetweenTaxes;

        /// <summary>
        /// The time until taxes; once each day
        /// </summary>
        [KSPField(isPersistant = true, guiActive = true,
                  guiName = "Time to rent payment", guiFormat = "F2")]
        public double TimeUntilTaxes = -1d;

        [KSPField(isPersistant = true, guiActive = false)]
        protected double lastUpdateTime = 0d;

        /// <summary>
        /// Taxes paid
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false)]
        public double TaxesPaid = 200d;

        protected void LogIt(string message)
        {
            string instance = GetInstanceID().ToString("X");
            Debug.Log(debuggingClass.modName + "(" + instance + ") " + message);
        }

        public override void OnAwake()
        {
            // Constructor logic goes here.
            LogIt("OnAwake");
            if (TimeUntilTaxes < 0.0)
                TimeUntilTaxes = TimeBetweenTaxes;
            if (TimeUntilGrowth < 0.0)
                TimeUntilGrowth = TimeBetweenGrowth;
        }

        public override void OnActive()
        {
            LogIt("OnActive: vessel = " + vessel);
        }

        public override void OnStart(PartModule.StartState state)
        {
            LogIt("OnStart: vessel = " + vessel + ", state = " + state);
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
            {
                foreach (BaseField field in Fields)
                    if (field.name == "TimeUntilTaxes")
                        field.guiActive = false;
            }
        }

        public override void OnUpdate()
        {
        }

        public override void OnFixedUpdate()
        {
        }

        public override string GetInfo()
        {
            return "";
        }

        public override void OnInactive()
        {
            LogIt("OnInactive: vessel = " + vessel);
        }

        protected double GetDeltaTime()
        {
            if (Time.timeSinceLevelLoad < 1.0f || !FlightGlobals.ready)
            {
                return -1;
            }

            if (Math.Abs(lastUpdateTime) < ResourceUtilities.FLOAT_TOLERANCE)
            {
                // Just started running
                lastUpdateTime = Planetarium.GetUniversalTime();
                return -1;
            }

            double maxDeltaTime = ResourceUtilities.GetMaxDeltaTime();
            double deltaTime = Math.Min(Planetarium.GetUniversalTime() - lastUpdateTime, maxDeltaTime);

            lastUpdateTime += deltaTime;
            return deltaTime;
        }
        ///////////////////////////////////////////////////////////////////////
        // Utility functions
        ///////////////////////////////////////////////////////////////////////

        protected double calculate_timestep(double x, double y, double z)
        {
            double timestep = x;
            if (y < timestep && y > 0.0)
                timestep = y;
            if (z < timestep && z > 0.0)
                timestep = z;
            return timestep;
        }

        /// <summary>
        /// Counts Civilians within parts implementing CivilianPopulationRegulator class.  This should be limited to only
        /// Civilian Population Parts.  It also only counts Kerbals with Civilian Population trait.  Iterates first over each
        /// part implementing CivilianPopulationRegulator, and then iterates over each crew member within that part.      
        /// </summary>
        /// <returns>The number of civilians on vessel</returns>
        public int countCivilians()
        {
            int numberCivilians = 0;
            foreach (ProtoCrewMember kerbalCrewMember in part.protoModuleCrew)
            {
                if (kerbalCrewMember.trait == debuggingClass.civilianTrait)
                    numberCivilians++;
            }
            return numberCivilians;
        }

        /// <summary>
        /// Counts non-Civilians within parts implementing CivilianPopulationRegulator class.  This should be limited to only
        /// Civilian Population Parts.  It also only counts Kerbals without Civilian Population trait.  Iterates first over each
        /// part implementing CivilianPopulationRegulator, and then iterates over each crew member within that part.      
        /// </summary>
        /// <returns>The number of non-civilians on vessel.</returns>
        public int countNonCivilians()
        {
            return part.protoModuleCrew.Count - countCivilians();
        }

        /// <summary>
        /// Counts the civilian seats on ship.
        /// </summary>
        /// <returns>The civilian seats on ship.</returns>
        public int countCivilianSeatsOnShip()
        {
            return part.CrewCapacity;
        }

        ///////////////////////////////////////////////////////////////////////
        // Taxes
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Calculates the rent based on the number of Kerbals within a ship.
        /// TODO:  Implement changing values after mod successfully altered
        /// </summary>
        /// <returns>The total rent.</returns>
        /// <param name="numberOfCivilians">Number of civilians.</param>
        int calculateRent(int numberOfCivilians)
        {
            return (int)(numberOfCivilians * TaxesPaid);
        }

        public void getTaxes(int numberOfCivilians)
        {
            int rentAcquired = calculateRent(numberOfCivilians);
            Funding.Instance.AddFunds(rentAcquired, TransactionReasons.Vessels);
            LogIt(rentAcquired + " has been paid in rent.");
            TimeUntilTaxes = 21600;
        }

        ///////////////////////////////////////////////////////////////////////
        // Population Growth
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This method will place a new civilian in a part containing CivlianPopulationRegulator.  It should only
        /// be called when there are seat positions open in onesuch part.  Perhaps in the future, there will be a specific
        /// part that generates Civilians.
        /// </summary>
        /// <param name="listOfMembers">List of members.</param>
        public void placeNewCivilian()
        {
            if (part.CrewCapacity == part.protoModuleCrew.Count())
                return;
            ProtoCrewMember newCivilian = createNewCrewMember(debuggingClass.civilianTrait);
            part.AddCrewmember(newCivilian);
            vessel.SpawnCrew();
            GameEvents.onVesselChange.Fire(vessel);
            LogIt(newCivilian.name + " has been added.");
        }

        /// <summary>
        /// Creates the new crew member of trait kerbalTraitName.  It must be of type Crew because they seem to be the only
        /// type of Kerbal that can keep a trait.
        /// </summary>
        /// <returns>The new crew member.</returns>
        /// <param name="kerbalTraitName">Kerbal trait name.</param>
        ProtoCrewMember createNewCrewMember(string kerbalTraitName)
        {
            KerbalRoster roster = HighLogic.CurrentGame.CrewRoster;
            ProtoCrewMember newKerbal = roster.GetNewKerbal(ProtoCrewMember.KerbalType.Crew);
            KerbalRoster.SetExperienceTrait(newKerbal, kerbalTraitName);//Set the Kerbal as the specified role (kerbalTraitName)
            Debug.Log(debuggingClass.modName + "Created " + newKerbal.name + ", a " + newKerbal.trait);
            return newKerbal;//returns newly-generated Kerbal
        }
    }
}