using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombCell : MonoBehaviour
{
    public int x, y; // Grid position
    public GameObject explosionEffectPrefab;

    public void SetGridPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.name = $"Bomb ({x}, {y})";
    }

    public void HandleClick()
    {
        Debug.Log($"Bomb at ({x}, {y}) clicked!");
        GameManager.Instance.GridManager.ActivateBomb(x, y); // Notify GridManager
    }

    public void ExplodeAnimation()
    {
        GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        // ✅ Force the particle system to play
        ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
        if (ps != null)
            ps.Play();
        else
            Debug.LogError("Particle System component is missing on the explosion prefab!");
        Destroy(explosion, 1f); // ✅ Auto-destroy after 1 second
    }
}

/*
    
*/
