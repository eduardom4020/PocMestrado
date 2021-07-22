using System.Collections.Generic;
using System.Linq;
using u2vis.Utilities;
using UnityEngine;

namespace u2vis
{
    public class StackedBar : BaseVisualizationView,IStackedVis
    {
        [SerializeField]
        private Mesh _dataItemMesh = null;
        [SerializeField]
        private int _valueIndex = 0;
        [SerializeField]
        private int _segments = 360;
        [SerializeField]
        private bool _useMinIndex = false;

        private float _divisor;
        public Mesh DataItemMesh
        {
            get { return _dataItemMesh; }
            set
            {
                _dataItemMesh = value;
                Rebuild();
            }
        }

        public int ValueIndex
        {
            get { return _valueIndex; }
            set
            {
                _valueIndex = value;
                Rebuild();
            }
        }

        #region Constructors
        protected StackedBar() : base()
        {
        }
        #endregion

        #region Protected Methods
        protected override void RebuildVisualization()
        {
            if (_presenter == null || _presenter.NumberOfDimensions < 3)
            {
                Debug.LogError("Presenter is either null or has not enough dimensions to represent this visualization");
                return;
            }

            var ticksX = GetCategoricalXAxisTicks();
            if(ticksX.Count() > 5)
            {
                Debug.LogError("Maximum categories limit exceded. The currrent maximum is 5");
                return;
            }

            if (_dataItemMesh == null)
            {
                _dataItemMesh = buildCircleMesh();
                Debug.Log("No DataMesh was set for this visualization. Using default");
            }
            var iMesh = new IntermediateMesh();
            // temporary save the mesh data from the template for faster access
            var tVertices = _dataItemMesh.vertices;
            var tNromals = _dataItemMesh.normals;
            var tUVs = _dataItemMesh.uv;
            var tIndices = _dataItemMesh.triangles;

            float divisor = 0;

            MultiDimDataPresenter presenter = (MultiDimDataPresenter)_presenter;

            for (int rowIndex = presenter.SelectedMinItem; rowIndex < presenter.SelectedMaxItem; rowIndex++)
            {
                float sum = 0;
                for (int dimIndex = 0; dimIndex < presenter.NumberOfDimensions; dimIndex++)
                {
                    sum += VisViewHelper.GetItemValueAbsolute(presenter, dimIndex, rowIndex);
                }
                divisor = Mathf.Max(divisor, sum);
            }
            float startHeight = 0;
            _divisor = divisor;

            var barWidth = 0.7f / ticksX.Count();
            var barsCenter = Enumerable.Range(0, presenter.TotalItemCount)
                .Select(i => i >= presenter.SelectedMinItem && i < presenter.SelectedMaxItem ? ticksX[i - presenter.SelectedMinItem].Position : 0.0f)
                .ToList();

            for (int rowIndex = presenter.SelectedMinItem; rowIndex < presenter.SelectedMaxItem; rowIndex++)
                for (int dimIndex = 0; dimIndex < _presenter.NumberOfDimensions; dimIndex++)
                {
                    if (dimIndex == 0)
                        startHeight = 0;

                    var dim = VisViewHelper.GetItemValueAbsolute(_presenter, dimIndex, rowIndex);
                    float height = dim / divisor;

                    var scale = new Vector3(barWidth, height * _size.y, _size.z);
                    var startIndex = iMesh.Vertices.Count;
                    foreach (var v in tVertices)
                    {
                        var vPos = new Vector3(barsCenter[rowIndex] - v.x * scale.x, startHeight * _size.y + v.y * scale.y, v.z * scale.z);
                        iMesh.Vertices.Add(vPos);
                        iMesh.Colors.Add(_style.GetColorCategorical(dimIndex, height));
                    }
                    iMesh.Normals.AddRange(tNromals);
                    iMesh.TexCoords.AddRange(tUVs);
                    foreach (var index in tIndices)
                        iMesh.Indices.Add(startIndex + index);
                    startHeight += height;
                }

            var mesh = iMesh.GenerateMesh("StackedBarMesh", MeshTopology.Triangles);
            var meshFilter = GetComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            var meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
                meshCollider.sharedMesh = mesh;
        }

        private Mesh buildCircleMesh()
        {
            Mesh mesh = new Mesh();
            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var indices = new List<int>();

            List<Vector3> partMeshTop = Circles.CreatePartMesh(0, 2 * Mathf.PI, 0.5f, _segments);
            for (int vertex = 0; vertex < partMeshTop.Count; vertex++)
            {
                partMeshTop[vertex] = new Vector3(partMeshTop[vertex].x, 1,partMeshTop[vertex].y);
            }
            vertices.AddRange(partMeshTop);
            List<Vector3> partMeshBottom = Circles.CreatePartMesh(0, 2 * Mathf.PI, 0.5f, _segments);
            for (int vertex = 0; vertex < partMeshBottom.Count; vertex++)
            {
                partMeshBottom[vertex] = new Vector3(partMeshBottom[vertex].x, 0,partMeshBottom[vertex].y);
            }
            vertices.AddRange(partMeshBottom);
            List<int> round = Circles.CreateRound(vertices.Count - partMeshBottom.Count - partMeshTop.Count, partMeshTop.Count -1,0);
            indices.AddRange(round);

            vertices.Add(new Vector3(0, 1, 0));
            List<Vector3> partMeshTopPlate = Circles.CreatePartMesh(0, 2 * Mathf.PI, 0.5f, _segments);
            partMeshTopPlate[0] = new Vector3(partMeshTopPlate[0].x,1, partMeshTopPlate[0].y);
            for (int vertex = 1; vertex < partMeshTopPlate.Count; vertex++)
            {
                partMeshTopPlate[vertex] = new Vector3(partMeshTopPlate[vertex].x,1, partMeshTopPlate[vertex].y);
                indices.Add(vertices.Count - 1);
                indices.Add(vertices.Count + vertex);
                indices.Add(vertices.Count - 1 + vertex);
            }
            vertices.AddRange(partMeshTopPlate);

            mesh.name = "CylinderTopPlate";
            mesh.vertices = vertices.ToArray();
            mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
            mesh.RecalculateNormals();
            return mesh;
        }
        #endregion

        private AxisTick[] GetCategoricalXAxisTicks()
        {
            MultiDimDataPresenter presenter = (MultiDimDataPresenter)_presenter;
            var xAxisDimensionName = presenter.AxisPresenters[0].Caption;
            var xAxisDataset = _presenter.DataProvider.Data.FirstOrDefault(x => x.Name == xAxisDimensionName);

            var ticksX = presenter.AxisPresenters[0].GenerateFromDimension(xAxisDataset, presenter.SelectedMinItem, presenter.SelectedMaxItem);

            return ticksX;
        }

        private string GetXAxisDimensionName()
        {
            MultiDimDataPresenter presenter = (MultiDimDataPresenter)_presenter;
            var xAxisDimensionName = presenter.AxisPresenters[0].Caption;
            return xAxisDimensionName;
        }

        protected override void RebuildAxes()
        {
            if (_axisViews == null || _fromEditor)
                SetupInitialAxisViews();

            var ticksX = GetCategoricalXAxisTicks();
            _axisViews[0].RebuildAxis(ticksX, GetXAxisDimensionName());

            var ticksY = new AxisTick[]
            {
                new AxisTick(0.10f, "10%"),
                new AxisTick(0.25f, "25%"),
                new AxisTick(0.5f, "50%"),
                new AxisTick(0.75f, "75%"),
                new AxisTick(0.9f, "90%")
            };

            _axisViews[1].RebuildAxis(ticksY, "%");
        }

        public virtual void Initialize(GenericDataPresenter presenter, GenericAxisView axisViewPrefab = null, GenericVisualizationStyle style = null, Mesh dataItemMesh = null)
        {
            _dataItemMesh = dataItemMesh;
            base.Initialize(presenter, axisViewPrefab, style);
        }

        public List<Vector3> GetSegmentStartList(Vector3 normDirectionVector)
        {
            List<Vector3> heights = new List<Vector3>();
            Vector3 locSc = this.gameObject.transform.localScale;
            float startHeight = 0;
            heights.Add(new Vector3(0,0,0));
            for (int dimIndex = 0; dimIndex < _presenter.NumberOfDimensions; dimIndex++)
            {
                float dim;
                if (_useMinIndex)
                {
                    dim = VisViewHelper.GetItemValueAbsolute(_presenter, dimIndex, _presenter.SelectedMinItem);
                }
                else
                {
                    dim = VisViewHelper.GetItemValueAbsolute(_presenter, dimIndex, _valueIndex);
                }
                float height = (dim / _divisor)*_size.y;
                startHeight += height;
                Vector3 heightVec = new Vector3(locSc.x * normDirectionVector.x * startHeight,locSc.y * normDirectionVector.y * startHeight, locSc.z * normDirectionVector.z * startHeight);
                heights.Add(heightVec);
            }
            return heights;
        }
    }
}
