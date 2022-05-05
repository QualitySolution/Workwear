using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using Workwear.Measurements;

namespace workwear.ViewModels.Stock
{
    public class WriteOffViewModel : EntityDialogViewModelBase<Writeoff>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public SizeService SizeService { get; }
        public INavigationManager NavigationManager { get; }
        public EmployeeCard CurWorker { get;}
        public Subdivision CurObject { get;}
        public Warehouse CurWarehouse { get; set; }

        public WriteOffViewModel(
            IEntityUoWBuilder uowBuilder, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            INavigationManager navigation,
            SizeService sizeService,
            IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
        {
            SizeService = sizeService;
            NavigationManager = navigation;
        }

        public WriteOffViewModel(
            Dictionary<Type, int> obkDictionary,
            IEntityUoWBuilder uowBuilder,
            IUnitOfWorkFactory unitOfWorkFactory,
            INavigationManager navigation,
            SizeService sizeService,
            IValidator validator = null) : this(uowBuilder, unitOfWorkFactory, navigation, sizeService)
        {
            if(obkDictionary.ContainsKey(typeof(EmployeeCard)))
                CurWorker = UoW.GetById<EmployeeCard>(obkDictionary[typeof(EmployeeCard)]);
            if (obkDictionary.ContainsKey(typeof(Subdivision)))
                CurObject = UoW.GetById<Subdivision>(obkDictionary[typeof(Subdivision)]);
        }

        #region ViewProperty
        public string Total => $"Позиций в документе: {Entity.Items.Count}  " +
                               $"Количество единиц: {Entity.Items.Sum(x => x.Amount)}";
        public bool DelSensitive { get; set; }
        
        #endregion
        #region Save
        public override bool Save() {
            Logger.Info ("Запись документа...");
            
            Entity.UpdateOperations(UoW);
            if (Entity.Id == 0)
                Entity.CreationDate = DateTime.Now;
            
            if(Entity.Items.Any(w => w.WriteoffFrom == WriteoffFrom.Employee)) {
                Logger.Debug ("Обновляем записи о выданной одежде в карточке сотрудника...");
                foreach(var employeeGroup in 
                    Entity.Items.Where(w => w.WriteoffFrom == WriteoffFrom.Employee)
                        .GroupBy(w => w.EmployeeWriteoffOperation.Employee))
                {
                    var employee = employeeGroup.Key;
                    foreach(var itemsGroup in 
                        employeeGroup.GroupBy(i => i.Nomenclature.Type.Id))
                    {
                        var wearItem = 
                            employee.WorkwearItems.FirstOrDefault(i => i.ProtectionTools.Id == itemsGroup.Key);
                        if(wearItem == null) {
                            Logger.Debug($"Позиции <{itemsGroup.First().Nomenclature.Type.Name}> не требуется к выдаче, пропускаем...");
                            continue;
                        }
                        wearItem.UpdateNextIssue (UoW);
                    }
                }
            }
            Logger.Info ("Ok");
            return base.Save();
        }
        #endregion
    }
}