using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SellShards.Settings;
using Silksong.FsmUtil;
using TeamCherry.Localization;

namespace SellShards.Helpers
{
    [HarmonyPatch(typeof(Fsm), "Awake")]
    public static class Fsm_Awake
    {
        [HarmonyPostfix]
        public static void Postfix(Fsm __instance)
        {
            if (!__instance.Name.Equals("Dialogue") ||
                !__instance.GameObjectName.Equals("Fixer Sitting Bone_10"))
            {
                return;
            }

            // Grab Repeat state so we can reference some of its variables
            FsmState repeat = __instance.GetState("Repeat");
            RunDialogue? runDialogue = FsmUtil.GetAction<RunDialogue>(repeat, 0);
            if (runDialogue == null)
            {
                return;
            }

            // Repeat -> EndRepeat stops the dialogue, but without returning player control
            FsmState endRepeatState = FsmUtil.AddState(__instance, "ShellShards_EndRepeat");
            FsmUtil.ChangeTransition(__instance, "Repeat", "CONVO_END", endRepeatState.Name);
            endRepeatState.AddAction(new EndDialogue()
            {
                ReturnControl = false,
                ReturnHUD = false,
                Target = runDialogue.Target,
                UseChildren = false
            });

            // EndRepeat -> Flick state does Flick's second dialogue asking if Hornet has any rosaries
            FsmState sellState = FsmUtil.AddState(__instance, "ShellShards_Flick");
            FsmUtil.AddTransition(__instance, endRepeatState.Name, "FINISHED", sellState.Name);
            sellState.AddAction(new RunDialogueV2() // Flick dialogue asking for shards
            {
                CustomText = Language.Get("FLICK_SELL_PROMPT", $"Mods.{SellShards.Id}"),
                Sheet = $"Mods.{SellShards.Id}",
                Key = "FLICK_SELL_PROMPT",
                OverrideContinue = false,
                PlayerVoiceTableOverride = runDialogue.PlayerVoiceTableOverride,
                PreventHeroAnimation = false,
                HideDecorators = false,
                TextAlignment = TMProOld.TextAlignmentOptions.TopLeft,
                OffsetY = 0,
                Target = runDialogue.Target
            });

            // Flick -> Yes/No state lets player decide whether or not to give Flick the shards
            FsmState yesNoState = FsmUtil.AddState(__instance, "ShellShards_YesNo");
            FsmUtil.AddTransition(__instance, sellState.Name, "CONVO_END", yesNoState.Name);
            yesNoState.AddAction(new EndDialogue()
            {
                ReturnControl = true,
                ReturnHUD = true,
                Target = runDialogue.Target,
                UseChildren = false
            });
            yesNoState.AddAction(new DialogueYesNoV2() // Yes/No prompt to sell the shards
            {
                TranslationSheet = $"Mods.{SellShards.Id}",
                TranslationKey = "YES_NO",
                UseCurrency = true,
                CurrencyCost = ConfigSettings.shardCost.Value,
                CurrencyType = CurrencyType.Shard,
                ConsumeCurrency = false,
                WillGetItem = new FsmObject(),
                YesEvent = FsmEvent.GetFsmEvent("YES"),
                NoEvent = FsmEvent.GetFsmEvent("NO"),
                ReturnHUDAfter = false
            });

            // Yes/No -> Yes gives the shards in exchange for rosaries
            FsmState yesState = FsmUtil.AddState(__instance, "ShellShards_Yes");
            FsmUtil.AddTransition(__instance, yesNoState.Name, "YES", yesState.Name);
            yesState.AddAction(new TakeCurrency() // Takes away the shards
            {
                CurrencyType = CurrencyType.Shard,
                Amount = ConfigSettings.shardCost.Value,
            });
            yesState.AddAction(new AddCurrency() // Gives the rosaries
            {
                CurrencyType = CurrencyType.Money,
                Amount = ConfigSettings.rosaries.Value,
            });

            // Yes/No -> No skips the Yes state and moves on to the Idle state, finishing the FSM
            FsmState noState = FsmUtil.AddState(__instance, "No");
            FsmUtil.AddTransition(__instance, yesNoState.Name, "NO", noState.Name);
            FsmUtil.AddTransition(__instance, yesState.Name, "FINISHED", noState.Name);
            FsmUtil.AddTransition(__instance, noState.Name, "FINISHED", "Idle");

            
        }
    }
}