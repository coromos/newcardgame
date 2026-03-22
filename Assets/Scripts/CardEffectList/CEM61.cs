using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Photon.Pun;


public class CEM61 : CardEffectManager
{
	public CEM61(CardController card) : base()
    {
        CardEffect effect = new CEDamage61(card);
        buckets[(int)CardEffectType.Grace].Add(effect);

    }

    class CEDamage61 : CEDamage
    {

        public CEDamage61(CardController card) : base(card)
        {
            damage = 14;
            cardAmount = 1;
        }

        public override void Activate()
        {
            Damage(damage, cardAmount);
        }

    }
}
