using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float lifeTime = 1f;
    public float fadeTime = 0.4f;

    private TextMesh textMesh;
    private Color startColor;
    private float timer;

    public static void Create(Vector3 position, int damage)
    {
        GameObject obj = new GameObject("Damage Popup");
        obj.transform.position = position;

        DamagePopup popup = obj.AddComponent<DamagePopup>();
        popup.Setup(damage);
    }

    void Setup(int damage)
    {
        textMesh = gameObject.AddComponent<TextMesh>();

        textMesh.text = "-" + damage;
        textMesh.fontSize = 64;
        textMesh.characterSize = 0.08f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = new Color(1f, 0.15f, 0.05f, 1f);

        startColor = textMesh.color;
    }

    void Update()
    {
        timer += Time.deltaTime;

        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        Camera cam = Camera.main;

        if (cam != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
        }

        if (timer > lifeTime - fadeTime)
        {
            float fadePercent = (timer - (lifeTime - fadeTime)) / fadeTime;
            Color color = startColor;
            color.a = Mathf.Lerp(1f, 0f, fadePercent);
            textMesh.color = color;
        }

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}