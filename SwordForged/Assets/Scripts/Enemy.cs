using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Creature 
{

	// Use this for initialization
	void Start () 
	{
		m_rb = GetComponent<Rigidbody2D>();

		m_hp = m_maxHealth;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(m_hp <= 0)
		{
			die();
		}
	}
}
