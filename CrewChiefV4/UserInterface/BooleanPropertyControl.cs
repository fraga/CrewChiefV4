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
    public partial class BooleanPropertyControl : UserControl
    {
        public bool changeRequiresRestart;
        public String propertyId;
        public String label;
        public Boolean defaultValue;
        public Boolean originalValue;
        private PropertiesForm parent;
        internal PropertyFilter filter = null;

        public BooleanPropertyControl(String propertyId, String label, Boolean value, Boolean defaultValue, String helpText, String filterText, 
            String categoryText, bool changeRequiresRestart, PropertiesForm parent)
        {
            InitializeComponent();
            this.parent = parent;
            this.label = label;
            this.propertyId = propertyId;
            this.originalValue = value;
            this.checkBox1.Text = label;
            this.checkBox1.Checked = value;
            this.defaultValue = defaultValue;
            this.toolTip1.SetToolTip(this.checkBox1, helpText);
            
            this.changeRequiresRestart = changeRequiresRestart;
            this.filter = new PropertyFilter(filterText, categoryText, changeRequiresRestart, propertyId, this.label);
        }
        public Boolean getValue()
        {
            return this.checkBox1.Checked;
        }

        public void setValue(Boolean value)
        {
            this.checkBox1.Checked = value;    
            this.originalValue = value;        
        }

        public void button1_Click(object sender, EventArgs e)
        {
            this.checkBox1.Checked = defaultValue;
            if (this.originalValue != this.checkBox1.Checked)
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

        private void checkedChanged(object sender, EventArgs e)
        {
            if (this.originalValue != this.checkBox1.Checked)
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
