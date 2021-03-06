﻿using S = System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[S.Serializable]
public struct Trait {
	public S.String name;
	public GameObject prefab;
	public Sprite[] variations;

	[Tooltip("-1 for none")]
	public int blancoId;
};

public class TraitController : MonoBehaviour {

	public Trait[] traits;

	List<Person> realPeople = new List<Person>();

	/* ------------- Traits --------------- */

	// Give a person random traits
	public void RandomizePerson(Person person) {
		if(!person.amIDisposable) {
            person.clones = new List<Person>();
            realPeople.Add(person);
			foreach(Trait t in traits) {
				int varId = Random.Range(0, t.variations.Length);
				GivePersonTrait(t, varId, person, true);
			}
		} else {
            Person clone = realPeople[Random.Range(0, realPeople.Count)];
			foreach(Trait t in traits) {
                int varId = clone.GetTraitVariation(t.name);
				GivePersonTrait(t, varId, person, true);
			}
            clone.clones.Add(person);
		}
	}

	// Give everybody random traits
	public void RandomizeEveryone() {
		foreach(var po in GameObject.FindGameObjectsWithTag("Person")) {
			RandomizePerson(po.GetComponent<Person>());
		};
	}

	private void GivePersonTrait(
			Trait t, int variantId, Person person, bool noAnimation=false) {
		Sprite variant = t.variations[variantId];
		person.SetTraitVariation(t.name, variantId, variant,
			variantId == t.blancoId, noAnimation);

        foreach(var clone in person.clones) {
            clone.SetTraitVariation(t.name, variantId, variant,
                variantId == t.blancoId, noAnimation);
        }
	}

	public Dictionary<int, int> GetTraitVariationsCount(S.String trait) {
		/// TODO
		return new Dictionary<int,int>();
	}

	public bool CheckIfEverybodyHaveSameColor(int color) {
		foreach (var po in GameObject.FindGameObjectsWithTag("Person")) {
			Person p = po.GetComponent<Person> ();
			int currPonchoId = p.GetTraitVariation ("poncho");
			if (currPonchoId != color) {
				return false;
			}
		}
		return true;
	}	

	public int CountColor(int color) {
		int count = 0;
		foreach (var po in GameObject.FindGameObjectsWithTag("Person")) {
			Person p = po.GetComponent<Person> ();
			int currPonchoId = p.GetTraitVariation ("poncho");
			if (currPonchoId == color)
				count++;
		}
		return count;
	}

	public Trait getTrait (string name) {
		return S.Array.Find<Trait> (traits, t => t.name == name);
	}

	public void TransitionTraits(GlyphSequence g) {
		// Harcoded to hats and ponchos,
		// modify the signature if the logic changes

		Trait hat = getTrait("hat");
		Trait poncho = getTrait("poncho");

		if(hat.name == null) Debug.LogError("hat trait not defined");
		if(poncho.name == null) Debug.LogError("poncho trait not defined");

		List<int> notPonchoBlancoIds = Enumerable.Range(0,poncho.variations.Length)
			.ToList().Except(new[] {poncho.blancoId}).ToList();

		foreach(var po in GameObject.FindGameObjectsWithTag("Person")) {
			Person p = po.GetComponent<Person>();

			if(p.amIDisposable) continue;

			int currHatId = p.GetTraitVariation("hat");
			int currPonchoId = p.GetTraitVariation("poncho");

			if(currHatId != g.hat) continue;

			if(poncho.blancoId < 0)
				Debug.LogError("Falta que implementar la lógica"
							 + " cuando no hay poncho blanco");

			if(poncho.blancoId >= poncho.variations.Length)
				Debug.LogError("Invalid poncho blanco id");

			//Poncho blanco
			if (currPonchoId == poncho.blancoId) {
				if (g.action == ActionGlyph.Up) {
					GivePersonTrait(poncho, g.poncho, p);
				} else if (g.action == ActionGlyph.Down) {
					int rnd = Random.Range(0, notPonchoBlancoIds.Count);
					GivePersonTrait(poncho, notPonchoBlancoIds[rnd], p);
				}
			}
			//Matchea
			else if (currPonchoId == g.poncho)
			{
				if (g.action == ActionGlyph.Down)
					GivePersonTrait(poncho, poncho.blancoId, p);
			}
			// No matchea
			else
			{
				if (g.action == ActionGlyph.Up)
					GivePersonTrait(poncho, poncho.blancoId, p);
			}
		};
	}
}
