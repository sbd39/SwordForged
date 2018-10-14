using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour 
{
	[SerializeField] protected int m_maxHealth;
	[SerializeField] protected float m_speed;
	[SerializeField] protected int m_damage;
	[SerializeField] protected float m_maxUnhitableTimer;
	protected Rigidbody2D m_rb;
	protected int m_hp;
	protected bool m_attacking = false;
	protected bool m_passable = false;
	protected bool m_hitable = true;
	protected float m_unhitableTimer = 0;

	protected virtual void die()
	{
		Destroy(gameObject);
	}

	protected virtual void becomeUnhitable()
	{
		m_hitable = false;
		m_passable = true;
		m_unhitableTimer = m_maxUnhitableTimer;
	}

	protected virtual void unhittableCooldown()
	{
		if (m_unhitableTimer <= 0)
		{
			m_passable = false;
			m_hitable = true;
		}
		else
		{
			m_unhitableTimer -= Time.deltaTime;
		}
	}
	
	public virtual void takeDamage(int damg, Vector2 dir = default(Vector2))
	{
		if (m_hitable)
		{
			m_hp -= damg;
		}
	}

	public virtual void heal(int hpRecovered)
	{
		if (hpRecovered + m_hp > m_maxHealth)
		{
			m_hp = m_maxHealth;
		}
		else
		{
			m_hp += hpRecovered;
		}
	}
}
