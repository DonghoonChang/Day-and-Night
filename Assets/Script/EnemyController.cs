using UnityEngine;


[RequireComponent(typeof(Animator))]
public class EnemyController : MonoBehaviour{

    /*
     * Controls Enemy Stats and Animation
     */
    [SerializeField] [Range(0, 150)] int m_Health = 150;

    public EnemyNavController navController;

    void Awake() {
    }


    void Update() {
    }

    public void TakeDamage (int basedamage, bool headshot)
    {
        int damage = headshot ? basedamage * 3 : basedamage;
        m_Health -= damage;
        if (m_Health == 0)
        {
            navController.isDead();
        }
    }
}
