﻿using Card.Client;
using Card.Effect;
using System;
using System.Collections.Generic;

namespace Card
{
    /// <summary>
    /// 法术卡牌
    /// </summary>
    [Serializable]
    public class AbilityCard : CardBasicInfo
    {
        /// <summary>
        /// 原生法术
        /// </summary>
        public const String 原生法术 = "0";
        /// <summary>
        /// 效果选择类型枚举
        /// </summary>
        public enum 效果选择类型枚举
        {
            /// <summary>
            /// 无需选择
            /// </summary>
            无需选择,
            /// <summary>
            /// 用户主动
            /// </summary>
            主动选择,
            /// <summary>
            /// 自动判定
            /// </summary>
            自动判定
        }
        /// <summary>
        /// 对象选择器
        /// </summary>
        public CardUtility.SelectOption AbliltyPosPicker = new CardUtility.SelectOption();
        /// <summary>
        /// 效果选择类型枚举
        /// </summary>
        public 效果选择类型枚举 效果选择类型 = 效果选择类型枚举.无需选择;
        /// <summary>
        /// 效果选择条件
        /// </summary>
        public String 效果选择条件 = String.Empty;
        /// <summary>
        /// 标准效果回数表达式
        /// </summary>
        public String 标准效果回数表达式 = String.Empty;
        /// <summary>
        /// 第一效果
        /// </summary>
        public AbilityDefine FirstAbilityDefine = new AbilityDefine();
        /// <summary>
        /// 第二效果
        /// </summary>
        public AbilityDefine SecondAbilityDefine = new AbilityDefine();
        /// <summary>
        /// 效果定义
        /// </summary>
        public struct AbilityDefine
        {
            /// <summary>
            /// 主效果定义
            /// </summary>
            public EffectDefine MainAbilityDefine;
            /// <summary>
            /// 追加效果定义
            /// </summary>
            public EffectDefine AppendAbilityDefine;
            /// <summary>
            /// 追加效果启动条件
            /// </summary>
            public String AppendEffectCondition;
            /// <summary>
            /// 初始化
            /// </summary>
            public void Init()
            {
                MainAbilityDefine = new EffectDefine();
                AppendAbilityDefine = new EffectDefine();
            }
            /// <summary>
            /// 用具体的类替换
            /// </summary>
            public void GetField()
            {
                MainAbilityDefine.TrueAtomicEffect.GetField();
                MainAbilityDefine.FalseAtomicEffect.GetField();
                AppendAbilityDefine.TrueAtomicEffect.GetField();
                AppendAbilityDefine.FalseAtomicEffect.GetField();
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public new void Init()
        {
            FirstAbilityDefine.GetField();
            SecondAbilityDefine.GetField();
        }
        /// <summary>
        /// 使用法术
        /// </summary>
        /// <param name="game"></param>
        /// <param name="ConvertPosDirect">对象方向转换</param>
        public List<String> UseAbility(GameManager game,
                                       Boolean ConvertPosDirect)
        {
            List<String> Result = new List<string>();
            Card.CardUtility.PickEffect PickEffectResult = CardUtility.PickEffect.第一效果;
            switch (效果选择类型)
            {
                case 效果选择类型枚举.无需选择:
                    break;
                case 效果选择类型枚举.主动选择:
                    PickEffectResult = game.PickEffect(FirstAbilityDefine.MainAbilityDefine.描述, SecondAbilityDefine.MainAbilityDefine.描述);
                    if (PickEffectResult == CardUtility.PickEffect.取消) return new List<string>();
                    break;
                case 效果选择类型枚举.自动判定:
                    if (!ExpressHandler.BattleFieldCondition(game, 效果选择条件)) PickEffectResult = CardUtility.PickEffect.第二效果;
                    break;
                default:
                    break;
            }
            List<EffectDefine> SingleEffectList = new List<EffectDefine>();
            AbilityCard.AbilityDefine ability;
            if (PickEffectResult == CardUtility.PickEffect.第一效果)
            {
                ability = FirstAbilityDefine;
            }
            else
            {
                ability = SecondAbilityDefine;
            }
            RunAbilityEffect(game, ConvertPosDirect, ability);
            return Result;
        }
        /// <summary>
        /// 运行法术
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="ConvertPosDirect"></param>
        /// <param name="Ability"></param>
        /// <param name="TargetPosInfo"></param>
        /// <returns></returns>
        private List<String> RunAbilityEffect(GameManager game,
                                              Boolean ConvertPosDirect,
                                              AbilityCard.AbilityDefine Ability)
        {
            List<String> Result = new List<string>();
            if (String.IsNullOrEmpty(Ability.MainAbilityDefine.效果条件))
            {
                //系统法术
                switch (Ability.MainAbilityDefine.TrueAtomicEffect.AtomicEffectType)
                {
                    case AtomicEffectDefine.AtomicEffectEnum.卡牌:
                    case AtomicEffectDefine.AtomicEffectEnum.水晶:
                    case AtomicEffectDefine.AtomicEffectEnum.奥秘:
                    case AtomicEffectDefine.AtomicEffectEnum.武器:
                        return RunGameSystemEffect(game,ConvertPosDirect,Ability);
                }
            }
            if (AbliltyPosPicker.EffictTargetSelectMode == CardUtility.TargetSelectModeEnum.指定 ||
                AbliltyPosPicker.EffictTargetSelectMode == CardUtility.TargetSelectModeEnum.横扫)
            {
                AbliltyPosPicker.SelectedPos = game.GetSelectTarget(AbliltyPosPicker, false);
            }
            else
            {
                if (ConvertPosDirect)
                {
                    switch (AbliltyPosPicker.EffectTargetSelectDirect)
                    {
                        case CardUtility.TargetSelectDirectEnum.本方:
                            AbliltyPosPicker.EffectTargetSelectDirect = CardUtility.TargetSelectDirectEnum.对方;
                            break;
                        case CardUtility.TargetSelectDirectEnum.对方:
                            AbliltyPosPicker.EffectTargetSelectDirect = CardUtility.TargetSelectDirectEnum.本方;
                            break;
                        case CardUtility.TargetSelectDirectEnum.双方:
                            break;
                        default:
                            break;
                    }
                }
            }
            if (AbliltyPosPicker.SelectedPos.Postion != -1)
            {
                //法术伤害对于攻击型效果的加成
                if (Ability.MainAbilityDefine.效果条件 == CardUtility.strIgnore && Ability.MainAbilityDefine.EffectCount > 1)
                {
                    Ability.MainAbilityDefine.EffectCount += game.MyInfo.BattleField.AttackEffectPlus;
                }
                //按照回数执行效果
                for (int cnt = 0; cnt < Ability.MainAbilityDefine.EffectCount; cnt++)
                {
                    Result.AddRange(Effecthandler.RunSingleEffect(AbliltyPosPicker,Ability.MainAbilityDefine, game, GameManager.RandomSeed));
                    GameManager.RandomSeed++;
                    Result.AddRange(game.Settle());
                }
            }
            else
            {
                //取消处理
                Result.Clear();
            }
            return Result;
        }
        /// <summary>
        /// 针对系统的法术效果
        /// </summary>
        /// <param name="game"></param>
        /// <param name="ConvertPosDirect"></param>
        /// <param name="Ability"></param>
        /// <returns></returns>
        private List<string> RunGameSystemEffect(GameManager game, bool ConvertPosDirect, AbilityDefine Ability)
        {
            throw new NotImplementedException();
        }
    }
}
