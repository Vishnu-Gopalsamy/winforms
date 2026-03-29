using APIwinforms.DataModel;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace APIwinforms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        public async void GetCategories()
        {
            try
            {
                HttpClient client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7171/api/Category");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var categories = JsonSerializer.Deserialize<List<CategoryModel>>(jsonString, options);
                comboBox1.DataSource = categories;
                comboBox1.DisplayMember = "Name";
                comboBox1.ValueMember = "Id";
                if (categories != null && categories.Count > 0)
                {
                    GetProducts(categories[0].Id);
                }
            }
            catch
            {
                comboBox1.DataSource = null;
            }
        }
        public async void GetProducts(int categoryId)
        {
            try
            {
                HttpClient client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:7171/api/Product/{categoryId}");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var products = JsonSerializer.Deserialize<List<ProductModel>>(jsonString, options);
                dataGridView1.DataSource = products;
            }
            catch
            {
                dataGridView1.DataSource = null;
            }
        }
        public async void LoadData()
        {
            GetCategories();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue is int categoryId)
            {
                GetProducts(categoryId);
            }
            else
            {
                dataGridView1.DataSource = null;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            
            if (comboBox1.SelectedValue is not int categoryId)
            {
                MessageBox.Show("Please select a category first.");
                return;
            }

            var newProduct = new
            {
                Name = "New product",
                Price = 0m,
                Image = (string?)null,
                CategoryId = categoryId
            };

            try
            {
                HttpClient client = new HttpClient();
                string json = JsonSerializer.Serialize(newProduct);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://localhost:7171/api/Product", content);
                response.EnsureSuccessStatusCode();

                // reload products for current category
                GetProducts(categoryId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            
            if (comboBox1.SelectedValue is not int categoryId)
            {
                MessageBox.Show("Please select a category first.");
                return;
            }

            if (dataGridView1.CurrentRow?.DataBoundItem is not ProductModel product)
            {
                MessageBox.Show("Please select a product to edit.");
                return;
            }

            var updatedProduct = new
            {
                product.Id,
                product.Name,
                product.Price,
                product.Image,
                CategoryId = categoryId
            };

            try
            {
                HttpClient client = new HttpClient();
                string json = JsonSerializer.Serialize(updatedProduct);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"https://localhost:7171/api/Product/{product.Id}", content);
                response.EnsureSuccessStatusCode();

                // reload products for current category
                GetProducts(categoryId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            // delete currently selected product
            if (comboBox1.SelectedValue is not int categoryId)
            {
                MessageBox.Show("Please select a category first.");
                return;
            }

            if (dataGridView1.CurrentRow?.DataBoundItem is not ProductModel product)
            {
                MessageBox.Show("Please select a product to delete.");
                return;
            }

            var confirm = MessageBox.Show("Delete selected product?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                HttpClient client = new HttpClient();
                var response = await client.DeleteAsync($"https://localhost:7171/api/Product/{product.Id}");
                response.EnsureSuccessStatusCode();

                // reload products for current category
                GetProducts(categoryId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
