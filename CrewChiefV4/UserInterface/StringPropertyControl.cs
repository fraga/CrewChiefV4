using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CrewChiefV4
{
    public partial class StringPropertyControl : UserControl
    {
        public bool changeRequiresRestart;
        public String propertyId;
        public String defaultValue;
        public String originalValue;
        public String label;
        private PropertiesForm parent;
        internal PropertyFilter filter = null;
        public StringPropertyControl(String propertyId, String label, String currentValue, String defaultValue, String helpText, String filterText,
            String categoryText, PropertiesForm parent)
        {
            InitializeComponent();
            this.parent = parent;
            this.label = label;
            this.propertyId = propertyId;
            this.label1.Text = label;
            this.originalValue = currentValue;
            this.textBox1.Text = currentValue;
            this.defaultValue = defaultValue;
            this.toolTip1.SetToolTip(this.textBox1, helpText);
            this.toolTip1.SetToolTip(this.label1, helpText);

            List<PropertiesForm.PropertyCategory> categoryList = PropertyFilter.parseCategories(categoryText);
            this.changeRequiresRestart = categoryList.Intersect(PropertiesForm.propsRequiringRestart).Count() > 0;

            this.filter = new PropertyFilter(filterText, categoryList, propertyId, this.label);
        }

        public String getValue()
        {
            return this.textBox1.Text;
        }

        public void setValue(String value)
        {
            this.originalValue = value;
            this.textBox1.Text = value;            
        }

        public void button1_Click(object sender, EventArgs e)
        {
            if (originalValue != defaultValue)
            {
                parent.hasChanges = true;
                if (this.changeRequiresRestart) parent.updatedPropertiesRequiringRestart.Add(this.propertyId);
            }
            else
            {
                parent.updatedPropertiesRequiringRestart.Remove(this.propertyId);
            }
            if (this.changeRequiresRestart) parent.updateSaveButtonText();
            this.textBox1.Text = defaultValue;
        }

        private void textChanged(object sender, EventArgs e)
        {
            if (this.textBox1.Text != originalValue)
            {
                parent.hasChanges = true;
                if (this.changeRequiresRestart) parent.updatedPropertiesRequiringRestart.Add(this.propertyId);
            }
            else
            {
                parent.updatedPropertiesRequiringRestart.Remove(this.propertyId);
            }
            if (this.changeRequiresRestart) parent.updateSaveButtonText();
        }
    }
}
