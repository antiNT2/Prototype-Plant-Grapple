using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable 
{
    void Damage(int damageAmount, Vector2 knockback, float damageAngle);
}
