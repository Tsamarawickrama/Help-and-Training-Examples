using System;
using System.Collections;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.SO;

namespace PhoneRepairShop
{
    public class RSSVPaymentPlanInq : PXGraph<RSSVPaymentPlanInq>
    {

        ...
		
        protected virtual IEnumerable detailsView()
        {
            BqlCommand query;
            var filter = Filter.Current;
            if (filter.GroupByStatus != true)
            {
                query = new SelectFrom<RSSVWorkOrderToPay>.
                  InnerJoin<ARInvoice>.On<ARInvoice.refNbr.IsEqual<RSSVWorkOrderToPay.invoiceNbr>>.
                  Where<RSSVWorkOrderToPay.status.IsNotEqual<workOrderStatusPaid>.
                    And<RSSVWorkOrderToPayFilter.customerID.FromCurrent.IsNull.
                      Or<RSSVWorkOrderToPay.customerID.IsEqual<
                        RSSVWorkOrderToPayFilter.customerID.FromCurrent>>>.
                      And<RSSVWorkOrderToPayFilter.serviceID.FromCurrent.IsNull.
                        Or<RSSVWorkOrderToPay.serviceID.IsEqual<
                          RSSVWorkOrderToPayFilter.serviceID.FromCurrent>>>>();
            }
            else
            {
                query = new SelectFrom<RSSVWorkOrderToPay>.
                  InnerJoin<ARInvoice>.On<ARInvoice.refNbr.IsEqual<RSSVWorkOrderToPay.invoiceNbr>>.
                  Where<RSSVWorkOrderToPay.status.IsNotEqual<workOrderStatusPaid>.
                    And<RSSVWorkOrderToPayFilter.customerID.FromCurrent.IsNull.
                      Or<RSSVWorkOrderToPay.customerID.IsEqual<
                        RSSVWorkOrderToPayFilter.customerID.FromCurrent>>>.
                      And<RSSVWorkOrderToPayFilter.serviceID.FromCurrent.IsNull.
                        Or<RSSVWorkOrderToPay.serviceID.IsEqual<
                          RSSVWorkOrderToPayFilter.serviceID.FromCurrent>>>>.
                    AggregateTo<GroupBy<RSSVWorkOrderToPay.status>, Sum<ARInvoice.curyDocBal>>();
            }

            var view = new PXView(this, true, query);
            foreach (PXResult<RSSVWorkOrderToPay, ARInvoice> order in view.SelectMulti(null))
            {
                yield return order;
            }

            var sorders = SelectFrom<SOOrderShipment>.
              InnerJoin<ARInvoice>.On<ARInvoice.refNbr.IsEqual<SOOrderShipment.invoiceNbr>>.
              Where<RSSVWorkOrderToPayFilter.customerID.FromCurrent.IsNull.
                Or<SOOrderShipment.customerID.IsEqual<
                  RSSVWorkOrderToPayFilter.customerID.FromCurrent>>>.
              View.Select(this);
            foreach (PXResult<SOOrderShipment, ARInvoice> order in sorders)
            {
                SOOrderShipment soshipment = order;
                ARInvoice invoice = order;
                RSSVWorkOrderToPay workOrder = RSSVWorkOrderToPay(soshipment);
                workOrder.OrderType = OrderTypeConstants.SalesOrder;
                var result = new PXResult<RSSVWorkOrderToPay, ARInvoice>(workOrder, invoice);
                yield return result;
            }
        }

		...


    }
}