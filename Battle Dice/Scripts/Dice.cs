using UnityEngine;
using HighlightingSystem;
using System.Collections;
using DG.Tweening;

public enum cubeValues
{
    None,
    Swords,
    Shield,
    Skull,
    Reroll,
}

public class Dice : MonoBehaviour {

    [SerializeField] private Light halo;
	public Highlighter highlighter;
	public MeshRenderer meshRenderer;
    private bool hasLanded = false;
    private int reRolled = 0;
    private int valueAfterThrow = 0;
    private Vector3 origin;

	Tweener colorTweener;
	private Color originalEmissionColor;

	Material instadMaterial;
    public bool HasLanded
    {
        get
        {
            return hasLanded;
        }

        set
        {
            hasLanded = value;
        }
    }

    public int ReRolled { get { return reRolled; }
        set
        {
            reRolled = value;
        }
    }

    public int ValueAfterThrow { get { return valueAfterThrow; }
        set
        {
            valueAfterThrow = value;
        }
    }

    public Light Halo
    {
        get
        {
            return halo;
        }

        set
        {
            halo = value;
        }
    }

    public Vector3 GetOrigin()
    {
        return origin;
    }

    public void SetOrigin(Vector3 value)
    {
        origin = value;
    }

	void Start()
	{
		instadMaterial = Instantiate<Material> (meshRenderer.material);
		meshRenderer.material = instadMaterial;
	}
   
	public void HighlightAndDehighlight(Color color)
	{
		highlighter.ConstantOn (color, 1);

		originalEmissionColor = instadMaterial.GetColor ("_EmissionColor");

		iTween.ValueTo (gameObject, iTween.Hash ("from", originalEmissionColor, "to", color, "time", 1, "onupdate", "OnEmissionUpdate"));
//		colorTweener = instadMaterial.DOColor (color, "_EmissionColor", 1f);
//		meshRenderer.material.SetColor(,);
		StartCoroutine (DeHighlight (1.5f));
	}

	void OnEmissionUpdate(Color newValue)
	{
		instadMaterial.SetColor ("_EmissionColor", newValue);
	}

	IEnumerator DeHighlight(float delay)
	{
		yield return new WaitForSeconds (delay);
		highlighter.ConstantOff (1);

		iTween.ValueTo (gameObject, iTween.Hash ("from", instadMaterial.GetColor ("_EmissionColor"), "to", originalEmissionColor, "time", 0.5f, "onupdate", "OnEmissionUpdate"));
//		instadMaterial.DOColor (originalEmissionColor, "_EmissionColor", 0.5f);
	}
}
