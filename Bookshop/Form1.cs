using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;

namespace Bookshop
{
    public partial class frmMain : Form
    {
        public static string status = "insert";
        List<Book> books = new List<Book>();
        public static Book current;
        SQLiteConnection conn;
        public string selectquery = "SELECT * FROM books;";

        public void loaddata()
        {
            lstBooks.Items.Clear();
            books.Clear();
            SQLiteCommand cmd = new SQLiteCommand(selectquery, conn);
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Book book;
                    book.id = reader.GetInt32(0);
                    book.name = reader.GetString(1);
                    book.author = reader.GetString(2);
                    book.publisher = reader.GetString(3);
                    book.category = reader.GetString(4);
                    book.price = reader.GetInt32(5);
                    book.description = reader.GetString(6);
                    book.image = reader.GetString(7);
                    books.Add(book);
                    BookItem item = new BookItem();
                    item.id = book.id;
                    item.name = book.name;
                    lstBooks.Items.Add(item);
                }
                reader.Close();
            } else
            {
                changestatus("insert");
                btnNext.Enabled = false;
                btnPrev.Enabled = false;
            }
        }

        public void filldata()
        {
            changestatus("update");
            txtName.Text = current.name;
            txtAuthor.Text = current.author;
            txtPublisher.Text = current.publisher;
            txtCategory.Text = current.category;
            txtPrice.Value = current.price;
            txtDescription.Text = current.description;
            picBook.ImageLocation = current.image;
            int indx = books.IndexOf(current);
            btnNext.Enabled = true;
            btnPrev.Enabled = true;
            if (indx == books.Count - 1)
                btnNext.Enabled = false;
            if (indx == 0)
                btnPrev.Enabled = false;
        }

        public void changecurrent(int id)
        {
            if(books.Count > 0) {
                current = books[0];
                for (int i = 0; i < books.Count; i++)
                {
                    if (books[i].id == id)
                    {
                        current = books[i];
                        break;
                    }
                }
            }
            
        }

        public void changestatus(string state)
        {
            status = state;
            if (status == "insert")
            {
                btnSave.Text = "افزودن";
                txtStatus.Text = "برای افزودن کتاب فیلد ها را پر کنید و روی افزودن کلیک کنید";
                txtName.Text = "";
                txtAuthor.Text = "";
                txtPublisher.Text = "";
                txtCategory.Text = "";
                txtPrice.Value = 0;
                txtDescription.Text = "";
                picBook.ImageLocation = "";
            }
            else if (status == "update")
            {
                btnSave.Text = "ذخیره";
                txtStatus.Text = "برای ویرایش اطلاعات فیلد ها را تغییر داده و روی ذخیره کلیک کنید";
            } else if(status == "search")
            {
                btnSave.Text = "جستجو";
                txtStatus.Text = "نام کتاب را وارد کنید و روی جستجو کلیک کنید";
                txtName.Text = "";
                txtAuthor.Text = "";
                txtPublisher.Text = "";
                txtCategory.Text = "";
                txtPrice.Value = 0;
                txtDescription.Text = "";
                picBook.ImageLocation = "";
            }
        }

        public frmMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            conn = new SQLiteConnection("Data Source=bookshop.db;Version=3;");
            conn.Open();
            SQLiteCommand cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS books (id INTEGER PRIMARY KEY AUTOINCREMENT, name text, author text, publisher text, category text, price int, description text, image text);", conn);
            cmd.ExecuteNonQuery();
            loaddata();
            if(books.Count > 0) {
                changecurrent(0);
                filldata();
            }
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            conn.Close();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            
            changestatus("insert");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SQLiteCommand cmd = null;
            int res = 0;
            switch (status)
            {
                case "insert":
                    cmd = new SQLiteCommand(String.Format("INSERT INTO books (name, author, publisher, category, price, description, image) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}');", txtName.Text, txtAuthor.Text, txtPublisher.Text, txtCategory.Text, txtPrice.Value, txtDescription.Text, picBook.ImageLocation), conn);
                    res = cmd.ExecuteNonQuery();
                    if (res > 0)
                    {
                        loaddata();
                        changecurrent(res);
                        filldata();
                        MessageBox.Show("کتاب جدید با موفقیت ثبت شد", "ثبت شد", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("کتاب جدید ثبت نشد ! دوباره سعی کنید", "ثبت نشد", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
                case "update":
                    cmd = new SQLiteCommand(string.Format("UPDATE books SET name='{0}', author='{1}', publisher='{2}', category='{3}', price='{4}', description='{5}', image='{6}' WHERE id='{7}';", txtName.Text, txtAuthor.Text, txtPublisher.Text, txtCategory.Text, txtPrice.Value, txtDescription.Text, picBook.ImageLocation, current.id), conn);
                    res = cmd.ExecuteNonQuery();
                    if (res > 0)
                    {
                        loaddata();
                        changecurrent(current.id);
                        filldata();
                        MessageBox.Show("اطلاعات کتاب با موفقیت بروز شد", "بروز شد", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("اطلاعات بروز نشد !‌ دوباره سعی کنید", "بروز نشد", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
                case "search":
                    string keyword = txtName.Text;
                    selectquery = String.Format("SELECT * FROM books WHERE name LIKE '%{0}%'", keyword);
                    loaddata();
                    if (books.Count > 0)
                    {
                        changecurrent(0);
                        filldata();
                    }
                    txtStatus.Text = String.Format("جستجو برای '{0}'", keyword);
                    break;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            int indx = books.IndexOf(current);
            if (indx < books.Count - 1)
            {
                current = books[indx + 1];
                filldata();
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            int indx = books.IndexOf(current);
            if (indx > 0)
            {
                current = books[indx - 1];
                filldata();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            changestatus("search");
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(String.Format("برای حذف \"{0}\"‌اطمینان دارید ؟", current.name), "حذف", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try { File.Delete(current.image); } catch { }
                SQLiteCommand cmd = new SQLiteCommand(String.Format("DELETE FROM books WHERE id='{0}';", current.id), conn);
                if(cmd.ExecuteNonQuery() > 0)
                {
                    loaddata();
                    if (books.Count > 0)
                    {
                        changecurrent(0);
                        filldata();
                    }
                }
                else
                {
                    MessageBox.Show("حذف نشد!");
                }
            }
        }

        private void lstBooks_SelectedIndexChanged(object sender, EventArgs e)
        {
            changecurrent(((BookItem)lstBooks.SelectedItem).id);
            filldata();
        }

        private void btnShowAll_Click(object sender, EventArgs e)
        {
            selectquery = "SELECT * FROM books;";
            loaddata();
            if(books.Count > 0)
            {
                changecurrent(0);
                filldata();
            }
            
        }

        private void picBook_Click(object sender, EventArgs e)
        {
            if(dialogSelectPic.ShowDialog() != DialogResult.Cancel)
            {
                try
                {
                    File.Copy(dialogSelectPic.FileName, "images/" + dialogSelectPic.SafeFileName);
                    picBook.ImageLocation = "images/" + dialogSelectPic.SafeFileName;
                }
                catch { }
            }

        }
    }
    public struct Book
    {
        public int id;
        public string name;
        public string author;
        public string publisher;
        public string category;
        public int price;
        public string description;
        public string image;
    }
    public class BookItem
    {
        public int id;
        public string name;
        public override string ToString()
        {
            return this.name;
        }
    }
}
