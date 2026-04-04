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