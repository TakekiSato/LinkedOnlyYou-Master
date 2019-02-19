using System.Collections;
using UnityEngine;

public class ModelLoad
{
    public IEnumerator Load(InitModel _modelName)
    {
        GameObject model = GetModelData(_modelName);
        yield return model;
        InstantiateModel(model);
    }

    private GameObject GetModelData(InitModel _modelName)
    {
        if (_modelName != InitModel.None)
        {
            string str = _modelName.ToString();
            Debug.Log(str);

            GameObject src = Resources.Load(str) as GameObject;

            return src;
        }
        else
        {
            Debug.Log("<color=red>" + "InitModel is None or Cant get _modelName" + "</color>");

            return null;
        }
    }

    private bool InstantiateModel(GameObject _src)
    {
        if (_src != null)
        {
            var instance = MonoBehaviour.Instantiate(_src).GetComponent<IModelInit>();
            instance.Init();

            return true;
        }
        return false;
    }
}
