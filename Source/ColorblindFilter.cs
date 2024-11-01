using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ColorAssist {
    public class ColorblindFilter :MonoBehaviour {

        [SerializeField] private bool _useFilter = true;

        private Material _mat;

        public bool UseFilter => _useFilter;

        private void Awake() {
            string filterName = ColorAssist.Instance.curFilter.Value;
            _mat = GetMaterialByName(filterName) ?? ColorAssist.Instance.colorBlindMaterials[0];
        }

        public void setMaterial(Material m) {
            _mat = m;
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dst) {
            if (_useFilter) {
                Graphics.Blit(src, dst, _mat);
            } else {
                Graphics.Blit(src, dst);
            }
        }

        private Material GetMaterialByName(string materialName) {
            foreach (var material in ColorAssist.Instance.colorBlindMaterials) {
                if (material.name == materialName) {
                    return material;
                }
            }
            return null; // Return null if no matching material is found
        }

        public void SetUseFilter(bool useFilter) =>
            _useFilter = useFilter;
    }
}
