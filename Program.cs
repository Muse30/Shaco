using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace Shaco
{
    internal class Shaco
    {
        public static Menu config;
        public static readonly AIHeroClient player = ObjectManager.Player;
        public static Spell.Targeted Q = new Spell.Targeted(SpellSlot.Q, 400);
        public static Spell.Skillshot W = new Spell.Skillshot(SpellSlot.W, 425, SkillShotType.Circular);
        public static Spell.Targeted E = new Spell.Targeted(SpellSlot.E, 625);
        public static Spell.Targeted R = new Spell.Targeted(SpellSlot.R, 2200);
        public static Spell.Active R2 = new Spell.Active(SpellSlot.R);
        public static Spell.Targeted SmiteSpell;

        public static Obj_AI_Base Monster;


        public static readonly string[] SmiteableUnits =
        {
            "SRU_Red", "SRU_Blue", "SRU_Dragon", "SRU_Baron",
            "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak",
            "SRU_Krug", "Sru_Crab"
        };

        private static readonly int[] SmiteRed = { 3715, 1415, 1414, 1413, 1412 };
        private static readonly int[] SmiteBlue = { 3706, 1403, 1402, 1401, 1400 };

        public static bool hasGhost = false;
        public static bool GhostDelay = false;
        public static int LastAATick;
        public static float cloneTime, lastBox;
        public static bool isDangerousSpell(string spellName, Obj_AI_Base target, Obj_AI_Base hero, Vector3 end, float spellRange)
                {
            if (spellName == "CurseofTheSadMummy")
            {
                if (player.Distance(hero.Position) <= 600f)
                {
                    return true;
                }
            }
            if (IsFacing(target, player.Position) &&
                (spellName == "EnchantedCrystalArrow" || spellName == "rivenizunablade" ||
                 spellName == "EzrealTrueshotBarrage" || spellName == "JinxR" || spellName == "sejuaniglacialprison"))
            {
                if (player.Distance(hero.Position) <= spellRange - 60)
                {
                    return true;
                }
            }
            if (spellName == "InfernalGuardian" || spellName == "UFSlash" ||
                (spellName == "RivenW" && player.HealthPercent< 25))
            {
                if (player.Distance(end) <= 270f)
                {
                    return true;
                }
            }
            if (spellName == "BlindMonkRKick" || spellName == "SyndraR" || spellName == "VeigarPrimordialBurst" ||
                spellName == "AlZaharNetherGrasp" || spellName == "LissandraR")
            {
                if (target.IsMe)
                {
                    return true;
                }
            }
            if (spellName == "TristanaR" || spellName == "ViR")
            {
                if (target.IsMe || player.Distance(target.Position) <= 100f)
                {
                    return true;
                }
            }
            if (spellName == "GalioIdolOfDurand")
            {
                if (player.Distance(hero.Position) <= 600f)
                {
                    return true;
                }
            }
            if (target != null && target.IsMe)
            {
                if (isTargetedCC(spellName) && spellName != "NasusW" && spellName != "ZedUlt")
                {
                    return true;
                }
            }
            return false;
        }

        public static bool isTargetedCC(string Spellname)
        {
            return TargetedCC.Contains(Spellname);
        }


        static void Main(string[] args)
        {
            Shaco hue = new Shaco();
            Loading.OnLoadingComplete += hue.ShacoLoad;
        }

        public void ShacoLoad(EventArgs args)
        {
            InitMenu();
            Chat.Print("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Shaco</font>");
            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnUpdate += SmiteEvent;
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base hero, GameObjectProcessSpellCastEventArgs args)
        {
            if (ShacoClone)
            {
                var clone = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(m => m.Name == player.Name && !m.IsMe);

                if (args == null || clone == null)
                {
                    return;
                }
                if (hero.NetworkId != clone.NetworkId)
                {
                    return;
                }
                LastAATick = Core.GameTickCount;
            }

            if (args == null || hero == null)
            {
                return;
            }
            if (ComboMenu["usercc"].Cast<CheckBox>().CurrentValue && hero is AIHeroClient && hero.IsEnemy &&
                player.Distance(hero) < Q.Range &&
                isDangerousSpell(
                    args.SData.Name, args.Target as AIHeroClient, hero, args.End, float.MaxValue))
            {
                R2.Cast();
            }
        
            if (hero.IsMe && args.SData.Name == "JackInTheBox")
            {
                lastBox = System.Environment.TickCount;
            }
        }

        public static bool IsFacing(Obj_AI_Base source, Vector3 target, float angle = 90)
        {
            if (source == null || !target.IsValid())
            {
                return false;
            }
            return
                (double)
                    Geometry.AngleBetween(
                        Geometry.Perpendicular(Extensions.To2D(source.Direction)), Extensions.To2D(target - source.Position)) <
                angle;
        }

        public static List<string> invulnerable =
            new List<string>(
                new string[]
                {
                    "sionpassivezombie", "willrevive", "BraumShieldRaise", "UndyingRage", "PoppyDiplomaticImmunity",
                    "LissandraRSelf", "JudicatorIntervention", "ZacRebirthReady", "AatroxPassiveReady", "Rebirth",
                    "alistartrample", "NocturneShroudofDarknessShield", "SpellShield"
                });

                public static List<string> TargetedCC =
                new List<string>(
               new string[]
                     {
                    "TristanaR", "BlindMonkRKick", "AlZaharNetherGrasp", "VayneCondemn", "JayceThunderingBlow", "Headbutt",
                    "Drain", "BlindingDart", "RunePrison", "IceBlast", "Dazzle", "Fling", "MaokaiUnstableGrowth",
                    "MordekaiserChildrenOfTheGrave", "ZedUlt", "LuluW", "PantheonW", "ViR", "JudicatorReckoning",
                    "IreliaEquilibriumStrike", "InfiniteDuress", "SkarnerImpale", "SowTheWind", "PuncturingTaunt",
                    "UrgotSwap2", "NasusW", "VolibearW", "Feast", "NocturneUnspeakableHorror", "Terrify", "VeigarPrimordialBurst"
                    });
        private void Game_OnGameUpdate(EventArgs args)
        {
            AIHeroClient target = TargetSelector.GetTarget(
                Q.Range + player.MoveSpeed * 3, DamageType.Physical);
            if (ShacoStealth && target != null && target.Health > ComboDamage(target) &&
                IsFacing(target, player.Position) &&
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Orbwalker.DisableAttacking = true;
            }
            else
            {
                Orbwalker.DisableAttacking = false;
            }
            if (!ShacoClone)
            {
                cloneTime = System.Environment.TickCount;
            }
            switch (Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Combo:
                    Combo(target);
                    break;
                case Orbwalker.ActiveModes.Harass:
                    Harass();
                    break;
                case Orbwalker.ActiveModes.LastHit:
                    break;
                default:
                    break;
            }
            if (E.IsReady())
            {
                var ksTarget =
                    EntityManager.Heroes.Enemies.FirstOrDefault(
                        h =>
                            h.IsValidTarget() && !h.Buffs.Any(b => invulnerable.Contains(b.Name)) &&
                            h.Health < ObjectManager.Player.GetSpellDamage(h, SpellSlot.E));
                if (ksTarget != null)
                {
                    if ((MiscMenu["ks"].Cast<CheckBox>().CurrentValue || MiscMenu["ksq"].Cast<CheckBox>().CurrentValue) &&
                        E.IsReady() && ksTarget.IsValidTarget(E.Range) && player.Mana > Player.GetSpell(SpellSlot.E).SData.Mana)
                    {
                        E.Cast(ksTarget);
                    }
                }
            }

            if (MiscMenu["stackBox"].Cast<KeyBind>().CurrentValue && W.IsReady())
            {
                var box =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(m => m.Distance(player) < W.Range && m.Name == "Jack In The Box" && !m.IsDead)
                        .OrderBy(m => m.Distance(Game.CursorPos))
                        .FirstOrDefault();

                if (box != null)
                {
                    W.Cast(box.Position);
                }
                else
                {
                    if (player.Distance(Game.CursorPos) < W.Range)
                    {
                        W.Cast(Game.CursorPos);
                    }
                    else
                    {
                        W.Cast(player.Position.Extend(Game.CursorPos, W.Range));
                    }
                }
            }
            if (ShacoClone && !GhostDelay && MiscMenu["autoMoveClone"].Cast<CheckBox>().CurrentValue)
            {
                moveClone();
            }
        }

        private void Combo(AIHeroClient target)
        {
            if (target == null)
            {
                return;
            }
            var cmbDmg = ComboDamage(target);
            float dist = (float)(Q.Range + player.MoveSpeed * 2.5);
            if (ShacoClone && !GhostDelay && ComboMenu["useClone"].Cast<CheckBox>().CurrentValue &&
                !MiscMenu["autoMoveClone"].Cast<CheckBox>().CurrentValue)
            {
                moveClone();
            }
            if ((ComboMenu["WaitForStealth"].Cast<CheckBox>().CurrentValue && ShacoStealth && cmbDmg < target.Health) ||
                !Orbwalker.CanMove)
            {
                return;
            }
            if (ComboMenu["useq"].Cast<CheckBox>().CurrentValue && Q.IsReady() &&
                Game.CursorPos.Distance(target.Position) < 250 && target.Distance(player) < dist &&
                (target.Distance(player) >= ComboMenu["useqMin"].Cast<Slider>().CurrentValue ||
                 (cmbDmg > target.Health && player.CountEnemiesInRange(2000) == 1)))
            {
                if (target.Distance(player) < Q.Range)
                {
                    Q.Cast(Prediction.Position.PredictUnitPosition(target, 500).To3D());
                }
                else
                {
                    if (!CheckWalls(target) || Utils.GetPath(player, target.Position) < dist)
                    {
                        Q.Cast(
                            player.Position.Extend(target.Position, Q.Range));
                    }
                }
            }
            bool hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlotFromName("SummonerDot")) == SpellState.Ready;
            if (ComboMenu["usew"].Cast<CheckBox>().CurrentValue && W.IsReady() && !target.IsUnderEnemyturret() &&
                target.Health > cmbDmg && player.Distance(target) < W.Range)
            {
                HandleW(target);
            }
            if (ComboMenu["usee"].Cast<CheckBox>().CurrentValue && E.IsReady() && target.IsValidTarget(E.Range))
            {
                E.Cast(target);
            }
            if (ComboMenu["user"].Cast<CheckBox>().CurrentValue && R.IsReady() && !ShacoClone && target.HealthPercent < 75 &&
                cmbDmg < target.Health && target.HealthPercent > cmbDmg && target.HealthPercent > 25)
            {
                R2.Cast();
            }
            if (ComboMenu["useIgnite"].Cast<CheckBox>().CurrentValue &&
                player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite) > target.Health && hasIgnite)
            {
                player.Spellbook.CastSpell(player.GetSpellSlotFromName("SummonerDot"), target);
            }
        }

        private void moveClone()
        {
            var Gtarget = TargetSelector.GetTarget(2200, DamageType.Physical);
            switch (MiscMenu["ghostTarget"].Cast<Slider>().CurrentValue)
            {
                case 0:
                    Gtarget = TargetSelector.GetTarget(2200, DamageType.Physical);
                    break;
                case 1:
                    Gtarget =
                        ObjectManager.Get<AIHeroClient>()
                            .Where(i => i.IsEnemy && !i.IsDead && player.Distance(i) <= 2200)
                            .OrderBy(i => i.Health)
                            .FirstOrDefault();
                    break;
                case 2:
                    Gtarget =
                        ObjectManager.Get<AIHeroClient>()
                            .Where(i => i.IsEnemy && !i.IsDead && player.Distance(i) <= 2200)
                            .OrderBy(i => player.Distance(i))
                            .FirstOrDefault();
                    break;
                default:
                    break;
            }

            if (Clone != null && Gtarget != null && Gtarget.IsValid && !Clone.Spellbook.IsAutoAttacking)
            {
                if (CanCloneAttack(Clone))
                {
                    R.Cast(Gtarget);
                }
                else if (player.HealthPercent > 25)
                {
                    var prediction = Prediction.Position.PredictUnitPosition(Gtarget, 2);
                    R.Cast(
                        Gtarget.Position.Extend(prediction.To3D(), Gtarget.GetAutoAttackRange()));
                }

                GhostDelay = true;
                Core.DelayAction(() => GhostDelay = false, 200);
            }
        }

        public static Obj_AI_Minion Clone
        {
            get
            {
                Obj_AI_Minion Clone = null;
                if (player.Spellbook.GetSpell(SpellSlot.R).Name != "HallucinateGuide") return null;
                return ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(m => m.Name == player.Name && !m.IsMe);
            }
        }

        private bool CheckWalls(AIHeroClient target)
        {
            var step = player.Distance(target) / 15;
            for (int i = 1; i < 16; i++)
            {
                if (player.Position.Extend(target.Position, step * i).IsWall())
                {
                    return true;
                }
            }
            return false;
        }

        private void HandleW(AIHeroClient target)
        {
            var turret =
                ObjectManager.Get<Obj_AI_Turret>()
                    .OrderByDescending(t => t.Distance(target))
                    .FirstOrDefault(t => t.IsEnemy && t.Distance(target) < 3000 && !t.IsDead);
            if (turret != null)
            {
                CastW(target, target.Position, turret.Position);
            }
            else
            {
                if (target.IsMoving)
                {
                    var pred = W.GetPrediction(target);
                    if (pred.HitChance >= HitChance.High)
                    {
                        CastW(target, target.Position, pred.UnitPosition);
                    }
                }
                else
                {
                    W.Cast(player.Position.Extend(target.Position, W.Range - player.Distance(target)));
                }
            }
        }

        public static bool CanCloneAttack(Obj_AI_Minion clone)
        {
            if (clone != null)
            {
                return Core.GameTickCount >=
                       LastAATick + Game.Ping + 100 + (clone.AttackDelay - clone.AttackCastDelay) * 1000;
            }
            return false;
        }

        private void CastW(AIHeroClient target, Vector3 from, Vector3 to)
        {
            var positions = new List<Vector3>();

            for (int i = 1; i < 11; i++)
            {
                positions.Add(from.Extend(to, 42 * i));
            }
            var best =
                positions.OrderByDescending(p => p.Distance(target.Position))
                    .FirstOrDefault(
                        p => !p.IsWall() && p.Distance(player.Position) < W.Range && p.Distance(target.Position) > 350);
            if (best != null && best.IsValid())
            {
                W.Cast(best);
            }
        }

        private static bool ShacoClone
        {
            get { return player.Spellbook.GetSpell(SpellSlot.R).Name == "HallucinateGuide"; }
        }

        private static bool ShacoStealth
        {
            get { return player.HasBuff("Deceive"); }
        }

        private void Harass()
        {
            AIHeroClient target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target == null)
            {
                return;
            }
            if (HarassMenu["useeH"].Cast<CheckBox>().CurrentValue && E.IsReady() && target.IsValidTarget(E.Range))
            {
                E.Cast(target);
            }
        }
        private float ComboDamage(AIHeroClient hero)
        {
            double damage = 0;

            if (Q.IsReady())
            {
                damage += player.GetSpellDamage(hero, SpellSlot.Q);
            }
            if (E.IsReady())
            {
                damage += player.GetSpellDamage(hero, SpellSlot.E);
            }


            var ignitedmg = player.GetSummonerSpellDamage(hero, DamageLibrary.SummonerSpells.Ignite);
            if (player.Spellbook.CanUseSpell(player.GetSpellSlotFromName("summonerdot")) == SpellState.Ready &&
                hero.Health < damage + ignitedmg)
            {
                damage += ignitedmg;
            }
            return (float)damage;
        }

        public static void SetSmiteSlot()
        {
            SpellSlot smiteSlot;
            if (SmiteBlue.Any(x => player.InventoryItems.FirstOrDefault(a => a.Id == (ItemId)x) != null))
                smiteSlot = player.GetSpellSlotFromName("s5_summonersmiteplayerganker");
            else if (
                SmiteRed.Any(
                    x => player.InventoryItems.FirstOrDefault(a => a.Id == (ItemId)x) != null))
                smiteSlot = player.GetSpellSlotFromName("s5_summonersmiteduel");
            else
                smiteSlot = player.GetSpellSlotFromName("summonersmite");
            SmiteSpell = new Spell.Targeted(smiteSlot, 500);
        }

        public static int GetSmiteDamage()
        {
            var level = player.Level;
            int[] smitedamage =
            {
                20*level + 370,
                30*level + 330,
                40*level + 240,
                50*level + 100
            };
            return smitedamage.Max();
        }

        private static void SmiteEvent(EventArgs args)
        {
            SetSmiteSlot();
            if (!SmiteSpell.IsReady() || player.IsDead) return;
            if (SmiteMenu["smiteActive"].Cast<KeyBind>().CurrentValue)
            {
                var unit =
                    EntityManager.MinionsAndMonsters.Monsters
                        .Where(
                            a =>
                                SmiteableUnits.Contains(a.BaseSkinName) && a.Health < GetSmiteDamage() &&
                                SmiteMenu[a.BaseSkinName].Cast<CheckBox>().CurrentValue)
                        .OrderByDescending(a => a.MaxHealth)
                        .FirstOrDefault();

                if (unit != null)
                {
                    SmiteSpell.Cast(unit);
                    return;
                }
            }
            if (SmiteMenu["useSlowSmite"].Cast<CheckBox>().CurrentValue &&
                SmiteSpell.Handle.Name == "s5_summonersmiteplayerganker")
            {
                foreach (
                    var target in
                        EntityManager.Heroes.Enemies
                            .Where(h => h.IsValidTarget(SmiteSpell.Range) && h.Health <= 20 + 8 * player.Level))
                {
                    SmiteSpell.Cast(target);
                    return;
                }
            }
            if (SmiteMenu["comboWithDuelSmite"].Cast<CheckBox>().CurrentValue &&
                SmiteSpell.Handle.Name == "s5_summonersmiteduel" &&
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                foreach (
                    var target in
                        EntityManager.Heroes.Enemies
                            .Where(h => h.IsValidTarget(SmiteSpell.Range)).OrderByDescending(TargetSelector.GetPriority)
                    )
                {
                    SmiteSpell.Cast(target);
                    return;
                }
            }
        }

        public static Menu MenuPrincipal, ComboMenu, HarassMenu, MiscMenu, SmiteMenu;
        private void InitMenu()
        {
            MenuPrincipal = MainMenu.AddMenu("Shaco", "Shaco");
            ComboMenu = MenuPrincipal.AddSubMenu("Combo settings", "Combo");
            ComboMenu.Add("useq", new CheckBox("Use Q"));
            ComboMenu.Add("useqMin", new Slider("Min range", 200, 0, 400));
            ComboMenu.Add("usew", new CheckBox("Use W"));
            ComboMenu.Add("usee", new CheckBox("Use E"));
            ComboMenu.Add("user", new CheckBox("Use R"));
            ComboMenu.Add("usercc", new CheckBox("Dodge targeted cc"));
            ComboMenu.Add("useClone", new CheckBox("Move clone"));
            ComboMenu.Add("WaitForStealth", new CheckBox("Block spells in stealth"));
            ComboMenu.Add("useIgnite", new CheckBox("Use Ignite"));

            // Harass Settings
            HarassMenu = MenuPrincipal.AddSubMenu("Harass settings", "harass");
            HarassMenu.Add("useeH", new CheckBox("Use E"));

            // Misc Settings
            MiscMenu = MenuPrincipal.AddSubMenu("Misc settings", "misc");
            StringList(MiscMenu, "ghostTarget", "Ghost target priority", new[] { "Targetselector", "Lowest health", "Closest to you" }, 0);
            MiscMenu.Add("ksq", new CheckBox("KS E"));
            MiscMenu.Add("autoMoveClone", new CheckBox("Always move clone"));
            MiscMenu.Add("stackBox", new KeyBind("Stack boxes", false, KeyBind.BindTypes.HoldActive, "T".ToCharArray()[0]));


            SmiteMenu = MenuPrincipal.AddSubMenu("Smite", "Smite");
            SmiteMenu.AddSeparator();
            SmiteMenu.Add("smiteActive",
                new KeyBind("Smite Active (toggle)", true, KeyBind.BindTypes.PressToggle, 'H'));
            SmiteMenu.AddSeparator();
            SmiteMenu.Add("useSlowSmite", new CheckBox("KS with Blue Smite"));
            SmiteMenu.Add("comboWithDuelSmite", new CheckBox("Combo with Red Smite"));
            SmiteMenu.AddSeparator();
            SmiteMenu.AddGroupLabel("Camps");
            SmiteMenu.AddLabel("Epics");
            SmiteMenu.Add("SRU_Baron", new CheckBox("Baron"));
            SmiteMenu.Add("SRU_Dragon", new CheckBox("Dragon"));
            SmiteMenu.AddLabel("Buffs");
            SmiteMenu.Add("SRU_Blue", new CheckBox("Blue"));
            SmiteMenu.Add("SRU_Red", new CheckBox("Red"));
            SmiteMenu.AddLabel("Small Camps");
            SmiteMenu.Add("SRU_Gromp", new CheckBox("Gromp", false));
            SmiteMenu.Add("SRU_Murkwolf", new CheckBox("Murkwolf", false));
            SmiteMenu.Add("SRU_Krug", new CheckBox("Krug", false));
            SmiteMenu.Add("SRU_Razorbeak", new CheckBox("Razerbeak", false));
            SmiteMenu.Add("Sru_Crab", new CheckBox("Skuttles", false));

        }
        public static void StringList(Menu menu, string uniqueId, string displayName, string[] values, int defaultValue)
        {
            var mode = menu.Add(uniqueId, new Slider(displayName, defaultValue, 0, values.Length - 1));
            mode.DisplayName = displayName + ": " + values[mode.CurrentValue];
            mode.OnValueChange +=
                delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                {
                    sender.DisplayName = displayName + ": " + values[args.NewValue];
                };
        }
    }
}