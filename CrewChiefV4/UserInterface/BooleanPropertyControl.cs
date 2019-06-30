﻿using System;
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
        internal PropertyFilter filter = null;
        public BooleanPropertyControl(String propertyId, String label, Boolean value, Boolean defaultValue, String helpText, String filterText, String categoryText)
        {
            InitializeComponent();

            this.label = label;
            this.propertyId = propertyId;
            this.originalValue = value;
            this.checkBox1.Text = label;
            this.checkBox1.Checked = value;
            this.defaultValue = defaultValue;
            this.toolTip1.SetToolTip(this.checkBox1, helpText);

            List<PropertiesForm.PropertyCategory> categoryList = PropertyFilter.parseCategories(categoryText);
            this.changeRequiresRestart = categoryList.Intersect(PropertiesForm.propsRequiringRestart).Count() > 0;
            this.filter = new PropertyFilter(filterText, categoryList, propertyId, this.label);
        }
        public Boolean getValue()
        {
            return this.checkBox1.Checked;
        }

        public void setValue(Boolean value)
        {
            this.originalValue = value;
            this.checkBox1.Checked = value;            
        }

        public void button1_Click(object sender, EventArgs e)
        {
            this.checkBox1.Checked = defaultValue;
            if (this.originalValue != this.checkBox1.Checked)
            {
                PropertiesForm.hasChanges = true;
                PropertiesForm.requiresRestart = this.changeRequiresRestart;
            }
        }

        private void checkedChanged(object sender, EventArgs e)
        {
            if (this.originalValue != this.checkBox1.Checked)
            {
                PropertiesForm.hasChanges = true;
                PropertiesForm.requiresRestart = this.changeRequiresRestart;
            }
        }
    }
}
