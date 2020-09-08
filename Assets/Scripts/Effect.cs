using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    public GameObject Effect1;
    public GameObject Effect2;

    public GameObject createPos;

    private void Start()
    {
        createPos = gameObject.transform.GetChild(0).gameObject;
    }

    public void eff1()
    {
        GameObject effect = Instantiate(Effect1, createPos.transform.position, createPos.transform.rotation);
        ParticleSystem particle = effect.GetComponent<ParticleSystem>();
        Destroy(effect, particle.main.duration);
    }

    public void eff2()
    {
        GameObject effect = Instantiate(Effect2, createPos.transform.position, createPos.transform.rotation);
        ParticleSystem particle = effect.GetComponent<ParticleSystem>();
        Destroy(effect, particle.main.duration);
    }
}
