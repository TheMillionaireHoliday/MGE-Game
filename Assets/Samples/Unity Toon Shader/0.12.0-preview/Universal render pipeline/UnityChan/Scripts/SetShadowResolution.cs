//
// SetShadowResolution.cs
// シャドウマップにカスタムレゾリューションを設定するスクリプト.
// メインライトにアタッチすること. 
//
namespace UnityEngine.Rendering.Toon.Universal.Samples
{
    public class SetShadowResolution : MonoBehaviour {
		public int resolution;
		
		void Update () {
				GetComponent<Light>().shadowCustomResolution = resolution;
		}
	}
}