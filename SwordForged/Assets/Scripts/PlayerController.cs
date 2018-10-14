using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Creature
{
	[SerializeField] private float attackRadius;
	[SerializeField] private LayerMask enemies;
	[SerializeField] private float maxKnockbackTimer;
	[SerializeField] private float maxDashTimer;
	[SerializeField] private float dashMultiplier;
	[SerializeField] private int jumpForce;
	[SerializeField] private float fallMultiplyer;
	[SerializeField] private float lowJumpMultiplyer;
	private Animator animator;
	private Transform attackPoint;
	private Vector2 knockbackDir;
	private bool damageMade = false;
	private bool knockedback = false;
	private bool jumping = false;
	private bool dashing = false;
	private bool facingRight = true;
	private float drag = 10;
	private float boundsX;
	private float boundsY;
	private float knockbackTimer = 0;
	private float dashTimer = 0;
	// Use this for initialization
	void Start () 
	{
		animator = GetComponent<Animator>();
		attackPoint = transform.GetChild(0);
		m_rb = GetComponent<Rigidbody2D>();

		m_hp = m_maxHealth;

		boundsX = GetComponent<BoxCollider2D>().bounds.extents.x;
		boundsY = GetComponent<BoxCollider2D>().bounds.extents.y;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(m_hp <= 0)
		{
			die();
		}
		if(!m_hitable)
		{
			unhittableCooldown();
		}
		if (knockedback)
		{
			knockback();
		}
		else
		{
			attack();
			move(Input.GetAxis("Horizontal"));
			dash();
			jump();
		}
	}

	private void attack()
	{
		if (!m_attacking)
		{
			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				m_attacking = true;
				animator.SetTrigger("Attack");
				dashing = false;
			}
		}
		else
		{
			if (attackPoint.gameObject.activeSelf == true)
			{
				if (damageMade == false)
				{
					damageMade = true;
					Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemies);
					if (hitObjects.Length > 0)
					{
						for (int i = 0; i < hitObjects.Length; i++)
						{
							print("hit enemy");
							hitObjects[i].gameObject.GetComponent<Creature>().takeDamage(m_damage);
						}
					}
				}
			}
			else
			{
				if (damageMade == true)
				{
					damageMade = false;
					m_attacking = false;
				}
			}
		}
	}

	public override void takeDamage(int damg, Vector2 dir)
	{
		if(m_hitable)
		{
			animator.SetTrigger("tookDamage");
			m_hp -= damg;
			becomeUnhitable();
			knockbackPlayer(dir.normalized);
		}
	}

	private void knockbackPlayer(Vector2 dir)
	{
		knockbackDir = dir;
		knockbackTimer = maxKnockbackTimer;
		knockedback = true;
	}

	private void knockback()
	{
		if (knockbackTimer <= 0)
		{
			knockedback = false;
		}
		else
		{
			knockbackTimer -= Time.deltaTime;
			m_rb.velocity = knockbackDir * 20;
		}
	}

	protected override void die()
	{
		animator.SetTrigger("Dead");
		Time.timeScale = 0;
	}

	private void move(float h)
	{
		//Direction that the player moves in
		if(h != 0)
		{
			animator.SetBool("Walking", true);
			float dir = h * m_speed;

			if (h < 0 && facingRight)
			{
				facingRight = false;
				transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
			}
			else if (h > 0 && !facingRight)
			{
				facingRight = true;
				transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
			}

			if (dashing && jumping)
			{
				m_rb.velocity = new Vector2(dir * dashMultiplier,m_rb.velocity.y);
			}
			else
			{
				m_rb.velocity = new Vector2(dir,m_rb.velocity.y);
			}
		}
		else
		{
			animator.SetBool("Walking", false);
			m_rb.velocity = new Vector2(m_rb.velocity.x * (1 - drag * Time.fixedDeltaTime),m_rb.velocity.y);
		}
	}

	private void dash()
	{
		if(!jumping && !m_attacking && Input.GetKeyDown(KeyCode.LeftShift))
		{
			dashing = true;
			animator.SetBool("Dashing", true);
			dashTimer = maxDashTimer;
		}

		if (dashing && !jumping)
		{
			if(dashTimer <= 0)
			{
				dashing = false;
				animator.SetBool("Dashing", false);
			}
			else
			{
				if(facingRight)
				{
					m_rb.velocity = Vector2.right * m_speed * dashMultiplier;
				}
				else
				{
					m_rb.velocity = Vector2.left * m_speed * dashMultiplier;
				}
				dashTimer -= Time.deltaTime;
			}
		}
	}

	private void jump()
	{
		//makes the player jump up by changing the velocity of the player in the y direction
		if (!jumping && Input.GetKeyDown(KeyCode.Space))
		{
			m_rb.velocity = Vector2.up * jumpForce;
			jumping = true;
			animator.SetBool("isJumping", true);
		}

		//Increases the player's decent while falling
		if (jumping && m_rb.velocity.y < 0)
		{
			m_rb.velocity += Vector2.up * Physics.gravity.y * (fallMultiplyer - 1) * Time.deltaTime;
		}
		//Makes the player not jump as high if they don't hold on to the space bar
		else if (m_rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
		{
			m_rb.velocity += Vector2.up * Physics.gravity.y * (lowJumpMultiplyer - 1) * Time.deltaTime;
		}
	}

	private bool checkGround()
	{
		for (float i = 0; i <= boundsX*2; i += 0.01f)
		{
			if (Physics2D.Raycast(new Vector3(transform.position.x - boundsX + i, transform.position.y, transform.position.z), Vector2.down, boundsY + 0.01f))
			{
				return true;
			}
		}
		return false;
	}

	private void OnCollisionEnter2D(Collision2D other) 
	{
		if (other.gameObject.CompareTag("Ground") && checkGround())
		{
			jumping = false;
			animator.SetBool("isJumping", false);
			if (dashing)
			{
				dashing = false;
				animator.SetBool("Dashing", false);
			}
		}
	}

	private void OnCollisionExit2D(Collision2D other) 
	{
		if (other.gameObject.CompareTag("Ground"))
		{
			jumping = true;
			animator.SetBool("isJumping", true);
		}
	}
}