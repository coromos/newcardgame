using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Photon.Pun;


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
{
    public CEM176() : base()
    {
        CardEffect effect = new CEAlive176();
        buckets[(int)CardEffectType.Alive].Add(effect);
    }
    class CEAlive176 : CEBuff
    {
        public CEAlive176() : base()
        {
            buffth = 0;
            buffpw = 5;
            buffdv = 5;
            cardAmount = 5;
        }
    }
}

public class CEM19 : CardEffectManager
{
    public CEM19() : base()
    {
        CardEffect effect = new CEAlive19();
        buckets[(int)CardEffectType.Alive].Add(effect);
    }
    class CEAlive19 : CEDraw
    {
         public CEAlive19() : base()
        {
            drawCount = 1;
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
            gameManager.CallGainThrive(hanei, ismine);
            yield return null;
        }
    }
}
