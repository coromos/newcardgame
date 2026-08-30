using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Photon.Pun;

public class CEM5 : CardEffectManager
{
    public CEM5() : base()
    {
        CardEffect effect = new CEDevote5();
        buckets[(int)CardEffectType.Devote].Add(effect);
    }
    class CEDevote5 : CEDraw
    {
        public CEDevote5() : base()
        {
            drawCount = 1;
        }
    }
}

public class CEM19 : CardEffectManager
/*
# ハイエナ
## 効果
- 1.【宣誓】1枚ドロー。
2.このアニマの捧身時、相手の繁栄を-5。
*/
{
    public CEM19() : base()
    {
        CardEffect effect = new CEAlive19();
        buckets[(int)CardEffectType.Alive].Add(effect);
        effect = new CEDevote19();
        buckets[(int)CardEffectType.Devote].Add(effect);
    }
    class CEAlive19 : CEDraw
    {
         public CEAlive19() : base()
        {
            drawCount = 1;
        }
    }
    
    class CEDevote19 : CardEffect
    {
        int hanei;
        bool ismine;
        public CEDevote19() : base()
        {
            hanei = -5;
            ismine = false;
        }

        public override IEnumerator Activate()
        {
            gameManager.CallGainThrive(hanei, !ismine ^ CSource.model.PlayerCard);
            yield return null;
        }
    }
}

public class CEM23 : CardEffectManager
{
    public CEM23() : base()
    {
        CardEffect effect = new CETrash23();
        buckets[(int)CardEffectType.Trash].Add(effect);
    }
    class CETrash23 : CEDraw
    {
        public CETrash23() : base()
        {
            drawCount = 2;
        }
    }
}

public class CEM26 : CardEffectManager
{
    public CEM26() : base()
    {
        CardEffect effect = new CEAlive26();
        buckets[(int)CardEffectType.Alive].Add(effect);
    }

    class CEAlive26 : CEDamageRandom
    {
        public CEAlive26() : base()
        {
            damage = 6;
            cardAmount = 2;
        }
    }
}

/*
# 地響き

## 効果
- 1.相手の場のカード全てに16ダメージ。
*/
public class CEM27 : CardEffectManager
{
    public CEM27() : base()
    {
        CardEffect effect = new CEGrace27();
        buckets[(int)CardEffectType.Grace].Add(effect);
    }

    class CEGrace27 : CEDamageRandom
    {
        public CEGrace27() : base()
        {
            damage = 16;
            cardAmount = 5;
            cardCategory = new CardCategory[] { CardCategory.Anima, CardCategory.Ornament };
        }
    }
}

public class CEM50 : CardEffectManager
{
    public CEM50() : base()
    {
        CardEffect effect = new CEAnyGrace50();
        buckets[(int)CardEffectType.AnyGrace].Add(effect);
    }
    class CEAnyGrace50 : CardEffect
    {
        int buffth;
        int buffpw;
        int buffdv;
        public CEAnyGrace50() : base()
        {
            buffth = 0;
            buffpw = 2;
            buffdv = 2;
        }

        public override IEnumerator Activate()
        {
            gameManager.CallBuffCard(CSource, buffth, buffpw, buffdv);
            yield return null;
        }
    }
}

public class CEM61 : CardEffectManager
{
	public CEM61() : base()
    {
        CardEffect effect = new CEDamage61();
        buckets[(int)CardEffectType.Grace].Add(effect);

    }

    class CEDamage61 : CEDamage
    {

        public CEDamage61() : base()
        {
            damage = 14;
            cardAmount = 1;
        }
    }
}

public class CEM73 : CardEffectManager
/*
防衛システム：【妨害】トークン召喚
*/
{
    public CEM73() : base()
    {
        CardEffect effect = new CEGrace73();
        buckets[(int)CardEffectType.Grace].Add(effect);
    }

    class CEGrace73 : CECreateCard
    {
        public CEGrace73() : base()
        {
            cardIds = new int[] { 71, 71 };
            place = PlaceList.Field;
            ismine = true;
        }
    }
}

public class CEM77 : CardEffectManager
/*
# カタパルト
## 効果
- 1.自分のターン開始時、ランダムな相手の場のアニマ1体に7ダメージ。
*/
{
    public CEM77() : base()
    {
        CardEffect effect = new CETurnStart77();
        buckets[(int)CardEffectType.TurnStart].Add(effect);
    }

    class CETurnStart77 : CEDamageRandom
    {
        public CETurnStart77() : base()
        {
            damage = 7;
            cardAmount = 1;
            cardCategory = new CardCategory[] { CardCategory.Anima };
        }

        public override IEnumerator Activate()
        {
            if (!(gameManager.isPlayerTurn ^ CSource.model.PlayerCard))
            {
                yield return base.Activate();
            }
        }
    }
}

public class CEM79 : CardEffectManager
/*
尖塔：自分のターン終了時、自分の繁栄を+4。殉難：自分の繁栄を-7。
*/
{
    public CEM79() : base()
    {
        CardEffect effect = new CETurnEnd79();
        buckets[(int)CardEffectType.TurnEnd].Add(effect);

        CardEffect effect2 = new CETrash79();
        buckets[(int)CardEffectType.Trash].Add(effect2);
    }

    class CETurnEnd79 : CardEffect
    {
        int hanei;
        bool ismine;
        public CETurnEnd79() : base()
        {
            hanei = 4;
            ismine = true;
        }

        public override IEnumerator Activate()
        {
            if (!(gameManager.isPlayerTurn ^ CSource.model.PlayerCard))
            {
                gameManager.CallGainThrive(hanei, !ismine ^ CSource.model.PlayerCard);
            }
            yield return null;
        }
    }

    class CETrash79 : CardEffect
    {
        int hanei;
        bool ismine;
        public CETrash79() : base()
        {
            hanei = -7;
            ismine = true;
        }

        public override IEnumerator Activate()
        {
            gameManager.CallGainThrive(hanei, !ismine ^ CSource.model.PlayerCard);
            yield return null;
        }
    }
}

public class CEM119 : CardEffectManager
{
    public CEM119() : base()
    {
        CardEffect effect = new CETurnStart119();
        buckets[(int)CardEffectType.TurnStart].Add(effect);
    }

    class CETurnStart119 : CardEffect
    {
        int hanei;
        bool ismine;
        public CETurnStart119() : base()
        {
            hanei = 4;
            ismine = true;
        }

        public override IEnumerator Activate()
        {
            if (!(gameManager.isPlayerTurn ^ CSource.model.PlayerCard))
            {
                gameManager.CallGainThrive(hanei, !ismine ^ CSource.model.PlayerCard);
            }
            yield return null;
        }
    }
}

public class CEM166 : CardEffectManager
{
    public CEM166() : base()
    {
        CardEffect effect = new CEAlive166();
        buckets[(int)CardEffectType.Alive].Add(effect);
    }

    class CEAlive166 : CECreateCard
    {
        public CEAlive166() : base()
        {
            cardIds = new int[] { 166 };
            place = PlaceList.Field;
            ismine = true;
        }
    }
}

public class CEM175 : CardEffectManager
{
    public CEM175() : base()
    {
        CardEffect effect = new CEAlive175();
        buckets[(int)CardEffectType.Alive].Add(effect);
    }
    class CEAlive175 : CEDamage
    {
        public CEAlive175() : base()
        {
            damage = 12;
            cardAmount = 1;
        }
    }
}

public class CEM176 : CardEffectManager
/*
時代の啓蒙者：全体バフ
*/
{
    public CEM176() : base()
    {
        CardEffect effect = new CEAlive176();
        buckets[(int)CardEffectType.Alive].Add(effect);
    }
    class CEAlive176 : CEBuffRandom
    {
        public CEAlive176() : base()
        {
            buffth = 0;
            buffpw = 5;
            buffdv = 5;
            cardAmount = 5;
            cardCategory = new CardCategory[] { CardCategory.Anima };
        }
    }
}

public class CEM222 : CardEffectManager
{
    public CEM222() : base()
    {
        CardEffect effect = new CEAlive222();
        buckets[(int)CardEffectType.Alive].Add(effect);
    }
    class CEAlive222 : CEDamage
    {
        public CEAlive222() : base()
        {
            damage = 13;
            cardAmount = 1;
        }
    }
}


