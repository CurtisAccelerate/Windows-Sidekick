namespace WindowsSidekick
{
    partial class PromptForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            promptInput = new TextBox();
            sendButton = new Button();
            ViewButton = new Button();
            buttonClose = new Button();
            SuspendLayout();
            // 
            // promptInput
            // 
            promptInput.Font = new Font("Cascadia Code", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            promptInput.Location = new Point(12, 56);
            promptInput.Multiline = true;
            promptInput.Name = "promptInput";
            promptInput.Size = new Size(606, 147);
            promptInput.TabIndex = 0;
            promptInput.Text = "Chat here";
            // 
            // sendButton
            // 
            sendButton.Font = new Font("Cascadia Code", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            sendButton.Location = new Point(527, 209);
            sendButton.Name = "sendButton";
            sendButton.Size = new Size(91, 37);
            sendButton.TabIndex = 1;
            sendButton.Text = "Send";
            sendButton.UseVisualStyleBackColor = true;
            // 
            // ViewButton
            // 
            ViewButton.Font = new Font("Cascadia Code", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ViewButton.Location = new Point(12, 12);
            ViewButton.Name = "ViewButton";
            ViewButton.Size = new Size(91, 37);
            ViewButton.TabIndex = 2;
            ViewButton.Text = "View";
            ViewButton.UseVisualStyleBackColor = true;
            // 
            // buttonClose
            // 
            buttonClose.Font = new Font("Cascadia Code", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buttonClose.Location = new Point(430, 209);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new Size(91, 37);
            buttonClose.TabIndex = 3;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            // 
            // PromptForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(630, 258);
            Controls.Add(buttonClose);
            Controls.Add(ViewButton);
            Controls.Add(sendButton);
            Controls.Add(promptInput);
            Name = "PromptForm";
            Text = "Sidekick";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox promptInput;
        private Button sendButton;
        private Button ViewButton;
        private Button buttonClose;
    }
}