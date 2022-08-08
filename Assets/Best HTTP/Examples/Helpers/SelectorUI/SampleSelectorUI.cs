﻿using System.Linq;
using UnityEngine;

namespace BestHTTP.Examples.Helpers.SelectorUI
{
    public class SampleSelectorUI : MonoBehaviour
    {
#pragma warning disable 0649

        [SerializeField]
        private Category _categoryListItemPrefab;

        [SerializeField]
        private ExampleListItem _exampleListItemPrefab;

        [SerializeField]
        private ExampleInfo _exampleInfoPrefab;

        [SerializeField]
        private RectTransform _listRoot;

        [SerializeField]
        private RectTransform _dyncamicContentRoot;

#pragma warning restore

        private SampleRoot sampleSelector;
        private ExampleListItem selectedSample;
        private GameObject dynamicContent;

        private void Start()
        {
            this.sampleSelector = FindObjectOfType<SampleRoot>();
            DisplayExamples();
        }

        private void DisplayExamples()
        {
            // Sort examples by category
            this.sampleSelector.samples.Sort((a, b) => {
                int result = a.Category.CompareTo(b.Category);
                if (result == 0)
                    result = a.DisplayName.CompareTo(b.DisplayName);
                return result;
            });

            string currentCategory = null;

            for (int i = 0; i < this.sampleSelector.samples.Count; ++i)
            {
                var examplePrefab = this.sampleSelector.samples[i];

                if (examplePrefab.BannedPlatforms.Contains(UnityEngine.Application.platform))
                    continue;

                if (currentCategory != examplePrefab.Category)
                {
                    var category = Instantiate<Category>(this._categoryListItemPrefab, this._listRoot, false);
                    category.SetLabel(examplePrefab.Category);

                    currentCategory = examplePrefab.Category;
                }

                var listItem = Instantiate<ExampleListItem>(this._exampleListItemPrefab, this._listRoot, false);
                listItem.Setup(this, examplePrefab);

                if (this.sampleSelector.selectedExamplePrefab == null)
                {
                    SelectSample(listItem);
                }
            }
        }

        public void SelectSample(ExampleListItem item)
        {
            this.sampleSelector.selectedExamplePrefab = item.ExamplePrefab;
            if (this.dynamicContent != null)
                Destroy(this.dynamicContent);

            var example = Instantiate<ExampleInfo>(this._exampleInfoPrefab, this._dyncamicContentRoot, false);
            example.Setup(this, item.ExamplePrefab);
            this.dynamicContent = example.gameObject;
        }

        public void ExecuteExample(SampleBase example)
        {
            if (this.dynamicContent != null)
                Destroy(this.dynamicContent);
            this.dynamicContent = Instantiate(example, this._dyncamicContentRoot, false).gameObject;
        }
    }
}