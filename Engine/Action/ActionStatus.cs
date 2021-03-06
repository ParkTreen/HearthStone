﻿using Engine.Card;
using Engine.Client;
using Engine.Effect;
using System;
using System.Collections.Generic;

namespace Engine.Action
{
    /// <summary>
    /// 动作用状态集合
    /// </summary>
    public class ActionStatus
    {
        /// <summary>
        /// 获得目标对象
        /// </summary>
        public static Utility.CSharpUtility.deleteGetTargetPosition GetSelectTarget;
        /// <summary>
        /// 抉择卡牌
        /// </summary>
        public static Utility.CSharpUtility.delegatePickEffect PickEffect;
        /// <summary>
        /// 全局随机种子
        /// </summary>
        public static int RandomSeed = 1;
        /// <summary>
        /// 全体角色
        /// </summary>
        public struct BattleRoles
        {
            public PublicInfo MyPublicInfo;
            public PrivateInfo MyPrivateInfo;
            public PublicInfo YourPublicInfo;
            public PrivateInfo YourPrivateInfo;
        }
        /// <summary>
        /// 全部角色
        /// </summary>
        public BattleRoles AllRole = new BattleRoles();
        /// <summary>
        /// 游戏编号
        /// </summary>
        public int GameId;
        /// <summary>
        /// 动作发起方是否为Host
        /// </summary>
        public bool IsHost;
        /// <summary>
        /// 事件处理
        /// </summary>
        public BattleEventHandler battleEvenetHandler = new BattleEventHandler();
        /// <summary>
        /// 当前中断
        /// </summary>
        public Control.FullServerManager.Interrupt Interrupt;
        /// <summary>
        /// 当前动作名称
        /// </summary>
        public String ActionName = String.Empty;
        /// <summary>
        /// 倒置
        /// </summary>
        public void Reverse()
        {
            PublicInfo TempPublic = AllRole.MyPublicInfo;
            PrivateInfo TempPrivate = AllRole.MyPrivateInfo;
            AllRole.MyPublicInfo = AllRole.YourPublicInfo;
            AllRole.MyPrivateInfo = AllRole.YourPrivateInfo;
            AllRole.YourPublicInfo = TempPublic;
            AllRole.YourPrivateInfo = TempPrivate;
            //关于方向的设置，还有战场位置
            AllRole.MyPublicInfo.Hero.战场位置.本方对方标识 = true;
            AllRole.YourPublicInfo.Hero.战场位置.本方对方标识 = false;
            foreach (var minion in AllRole.MyPublicInfo.BattleField.BattleMinions)
            {
                if (minion != null) minion.战场位置.本方对方标识 = true;
            }
            foreach (var minion in AllRole.YourPublicInfo.BattleField.BattleMinions)
            {
                if (minion != null) minion.战场位置.本方对方标识 = false;
            }
        }
        /// <summary>
        /// 清算(核心方法)
        /// </summary>
        /// <returns></returns>
        public static List<string> Settle(ActionStatus game)
        {
            //每次原子操作后进行一次清算
            //将亡语效果也发送给对方
            List<string> actionlst = new List<string>();
            //1.检查需要移除的对象
            var MyDeadMinion = game.AllRole.MyPublicInfo.BattleField.ClearDead(game.battleEvenetHandler, true);
            var YourDeadMinion = game.AllRole.YourPublicInfo.BattleField.ClearDead(game.battleEvenetHandler, false);
            //2.重新计算Buff
            Buff.ResetBuff(game);
            //3.武器的移除
            if (game.AllRole.MyPublicInfo.Hero.Weapon != null && game.AllRole.MyPublicInfo.Hero.Weapon.耐久度 == 0) game.AllRole.MyPublicInfo.Hero.Weapon = null;
            if (game.AllRole.YourPublicInfo.Hero.Weapon != null && game.AllRole.YourPublicInfo.Hero.Weapon.耐久度 == 0) game.AllRole.YourPublicInfo.Hero.Weapon = null;
            //发送结算同步信息
            actionlst.Add(Server.ActionCode.strSettle);
            foreach (var minion in MyDeadMinion)
            {
                //亡语的时候，本方无需倒置方向
                actionlst.AddRange(minion.发动亡语(game));
            }
            //互换本方对方
            game.Reverse();
            foreach (var minion in YourDeadMinion)
            {
                //亡语的时候，对方需要倒置方向
                //例如，亡语为 本方召唤一个随从，敌人亡语，变为敌方召唤一个随从
                actionlst.AddRange(minion.发动亡语(game));
            }
            //保持本方对方
            game.Reverse();
            return actionlst;
        }
    }
}
