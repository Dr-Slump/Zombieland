﻿using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace ZombieLand
{
	public class Dialog_ZombieDebugActionMenu : Dialog_DebugActionsMenu
	{
		private void SpawnZombie(ZombieType type, bool appearDirectly)
		{
			ZombieGenerator.SpawnZombie(UI.MouseCell(), Find.CurrentMap, type, (zombie) =>
			{
				if (appearDirectly)
				{
					zombie.rubbleCounter = Constants.RUBBLE_AMOUNT;
					zombie.state = ZombieState.Wandering;
				}
				zombie.Rotation = Rot4.South;

				var tickManager = Find.CurrentMap.GetComponent<TickManager>();
				tickManager.allZombiesCached.Add(zombie);
			});
		}

		protected override void DoListingItems()
		{
			base.DoListingItems();

			if (Current.ProgramState != ProgramState.Playing)
				return;

			var map = Find.CurrentMap;
			if (map == null)
				return;

			DoGap();
			DoLabel("Tools - Zombies");

			DebugToolMap("Spawn: Zombie (dig out)", delegate
			{
				SpawnZombie(ZombieType.Normal, false);
			});
			DebugToolMap("Spawn: Zombie (standing)", delegate
			{
				SpawnZombie(ZombieType.Normal, true);
			});
			DebugToolMap("Spawn: Suicide bomber", delegate
			{
				SpawnZombie(ZombieType.SuicideBomber, true);
			});
			DebugToolMap("Spawn: Toxic Splasher", delegate
			{
				SpawnZombie(ZombieType.ToxicSplasher, true);
			});
			DebugToolMap("Spawn: Tanky Operator", delegate
			{
				SpawnZombie(ZombieType.TankyOperator, true);
			});
			DebugToolMap("Spawn: Miner", delegate
			{
				SpawnZombie(ZombieType.Miner, true);
			});
			DebugToolMap("Spawn: Electrifier", delegate
			{
				SpawnZombie(ZombieType.Electrifier, true);
			});
			DebugToolMap("Spawn: Albino", delegate
			{
				SpawnZombie(ZombieType.Albino, true);
			});
			DebugToolMap("Spawn: Random zombie", delegate
			{
				SpawnZombie(ZombieType.Random, true);
			});
			DebugToolMap("Spawn: Zombie incident (4)", delegate
			{
				_ = ZombiesRising.TryExecute(map, 4, UI.MouseCell(), true);
			});
			DebugToolMap("Spawn: Zombie incident (25)", delegate
			{
				_ = ZombiesRising.TryExecute(map, 25, UI.MouseCell(), true);
			});
			DebugToolMap("Spawn: Zombie incident (100)", delegate
			{
				_ = ZombiesRising.TryExecute(map, 100, UI.MouseCell(), true);
			});
			DebugToolMap("Spawn: Zombie incident (200)", delegate
			{
				_ = ZombiesRising.TryExecute(map, 200, UI.MouseCell(), true);
			});
			DebugToolMap("Convert: Make Zombie", delegate
			{
				foreach (var thing in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
				{
					var pawn = thing as Pawn;
					if (pawn == null || pawn is Zombie)
						continue;
					Tools.ConvertToZombie(pawn, true);
				}
			});
			DebugToolMap("Apply: Trigger rotting", delegate
			{
				foreach (var thing in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
				{
					var compRottable = thing.TryGetComp<CompRottable>();
					if (compRottable != null)
						compRottable.RotProgress = compRottable.PropsRot.TicksToRotStart;
				}
			});
			DebugToolMap("Apply: Add infection", delegate
			{
				foreach (var thing in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
				{
					var pawn = thing as Pawn;
					if (pawn == null || pawn is Zombie)
						continue;

					var bodyPart = pawn.health.hediffSet
						.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
						.FirstOrDefault(part => part.IsCorePart == false);
					if (bodyPart == null)
						bodyPart = pawn.health.hediffSet
						.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).SafeRandomElement();

					var def = HediffDef.Named("ZombieBite");
					var bite = (Hediff_Injury_ZombieBite)HediffMaker.MakeHediff(def, pawn, bodyPart);

					bite.mayBecomeZombieWhenDead = true;
					bite.TendDuration.ZombieInfector.MakeHarmfull();
					var damageInfo = new DamageInfo(Tools.ZombieBiteDamageDef, 2);
					pawn.health.AddHediff(bite, bodyPart, damageInfo);
				}
			});
			DebugToolMap("Apply: Remove infection", delegate
			{
				foreach (var thing in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
				{
					var pawn = thing as Pawn;
					if (pawn == null || pawn is Zombie)
						continue;
					pawn.health.hediffSet
						.GetHediffs<Hediff_Injury_ZombieBite>()
						.Do(bite =>
						{
							bite.mayBecomeZombieWhenDead = false;
							var tendDuration = bite.TryGetComp<HediffComp_Zombie_TendDuration>();
							tendDuration.ZombieInfector.MakeHarmless();
						});
				}
			});
			DebugToolMap("Apply: Zombie raging", delegate
			{
				foreach (var thing in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
				{
					var zombie = thing as Zombie;
					if (zombie == null)
						continue;
					ZombieStateHandler.StartRage(zombie);
				}
			});
		}
	}
}